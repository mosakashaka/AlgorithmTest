using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AlgorithmTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatorController : ControllerBase
    {
        private readonly ILogger<CalculatorController> _logger;

        public CalculatorController(ILogger<CalculatorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public decimal Calculate([FromQuery] string expr)
        {
            var calc = new Calculator(expr);
            return calc.calculate();
        }

        #region Calculator

        // type of the Argument
        public enum ArgType
        {
            //brackets begin end
            BRACKET_BEGIN,

            BRACKET_END,
            NUMBER,
            OP,
            INVALID
        }

        //math operator type
        public enum OperatorType
        {
            INVALID,
            PLUS,
            MINUS,
            MULTIPLY,
            DIVIDE
        }

        internal class Calculator
        {
            #region Fields

            private string Expression { get; set; }

            private ArgType LastType { get; set; }

            private Stack<Argument> Args = new Stack<Argument>();

            public static Dictionary<char, OperatorType> Operators = new Dictionary<char, OperatorType>()
        {
            {'+', OperatorType.PLUS },
            {'-', OperatorType.MINUS },
            {'*', OperatorType.MULTIPLY },
            {'/', OperatorType.DIVIDE }
        };

            //fields for resolving
            private List<Argument> ArgList = new List<Argument>();

            //position for each start of a bracket
            private List<int> Brackets = new List<int>();

            //1:waiting for left number, 2: operator or done, 3: right number
            private int State = 3;

            private ArgType OldType = ArgType.INVALID;
            private ArgType CurrentType = ArgType.INVALID;

            #endregion Fields

            #region private methods

            //parse and calculate
            //1. mark the start of a bracket
            //2. only calculate part of an expression if we meet the end of a bracket, and till the begin of the relative bracket
            private void parse()
            {
                if (string.IsNullOrEmpty(Expression))
                {
                    throw new ArgumentException("Invalid input");
                }

                var end_pos = Expression.Length;
                var pos = 0;

                //parse till next token
                while (pos < end_pos)
                {
                    var c = Expression[pos++];
                    if (char.IsDigit(c))
                    {
                        CurrentType = ArgType.NUMBER;
                    }
                    else if (Operators.ContainsKey(c))
                    {
                        CurrentType = ArgType.OP;
                    }
                    else if ('('.Equals(c))
                    {
                        CurrentType = ArgType.BRACKET_BEGIN;
                        Brackets.Add(ArgList.Count);
                    }
                    else if (')'.Equals(c))
                    {
                        CurrentType = ArgType.BRACKET_END;
                    }
                    else
                    {
                        throw new ArgumentException($"The char is not supported:{c}");
                    }

                    if (OldType == ArgType.INVALID)
                    {
                        handleNewArg(c);
                    }
                    else if (OldType == ArgType.NUMBER)
                    {
                        if (OldType == CurrentType)
                        {
                            handleOldArg(c);
                        }
                        else
                        {
                            handleNewArg(c);
                        }
                    }
                    else
                    {
                        handleNewArg(c);
                    }
                }
                doCalculate(ArgList);
            }

            private void handleNewArg(char c)
            {
                OldType = CurrentType;

                if (CurrentType == ArgType.BRACKET_END)
                {
                    if (Brackets.Count == 0)
                    {
                        throw new Exception($"invalid bracket end!");
                    }
                    var lastBracket = Brackets[Brackets.Count - 1];
                    var result = doCalculate(ArgList.Skip(lastBracket).ToList());
                    Brackets.RemoveAt(Brackets.Count - 1);

                    //truncate and convert to a result
                    ArgList.RemoveRange(lastBracket, ArgList.Count - lastBracket);
                    ArgList.Add(new Argument()
                    {
                        Type = ArgType.NUMBER,
                        Value = result
                    });
                }
                else
                {
                    var arg = new Argument()
                    {
                        Type = CurrentType
                    };

                    arg.Assign(c);
                    ArgList.Add(arg);
                }
            }

            private void handleOldArg(char c)
            {
                //only support number
                ArgList.Last().Assign(c);
            }

            private decimal doCalculate(List<Argument> list)
            {
                list.RemoveAll(p => (p.Type == ArgType.BRACKET_BEGIN) || (p.Type == ArgType.BRACKET_END));

                //multiply and divide
                for (var i = 0; i <= 1; i++)
                {
                    var op1 = OperatorType.DIVIDE;
                    var op2 = OperatorType.MULTIPLY;
                    if (i == 1)
                    {
                        //*/ first, +- second
                        op1 = OperatorType.MINUS;
                        op2 = OperatorType.PLUS;
                    }
                    for (var idx = 0; idx < list.Count; idx++)
                    {
                        var current = list[idx];
                        if (current == null)
                        {
                            break;
                        }
                        if ((current.Type != ArgType.OP)
                            || ((current.OpType != op1)
                                && (current.OpType != op2))
                                )
                        {
                            continue;
                        }
                        var r = getResult(list[idx - 1], list[idx], list[idx + 1]);
                        //we rely on 3 nodes around us to calculate a value [idx-1]:left  [idx]:op [idx+1]:right
                        //so we need to remove 2 nodes, and replace one of them with the result
                        idx--;
                        list[idx] = new Argument()
                        {
                            Type = ArgType.NUMBER,
                            Value = r
                        };
                        list.RemoveRange(idx + 1, 2);
                    }
                }
                if (list.Count != 1)
                {
                    throw new Exception($"invalid calc result: [{JsonSerializer.Serialize(list)}]");
                }

                return list[0].Value;
            }

            //
            private decimal getResult(Argument left, Argument op, Argument right)
            {
                left.Ensure(ArgType.NUMBER);
                op.Ensure(ArgType.OP);
                right.Ensure(ArgType.NUMBER);
                System.Console.WriteLine($"Calculate [{left.Value}][{op.OpType}][{right.Value}]");

                switch (op.OpType)
                {
                    case OperatorType.PLUS:
                        return left.Value + right.Value;

                    case OperatorType.MINUS:
                        return left.Value - right.Value;

                    case OperatorType.MULTIPLY:
                        return left.Value * right.Value;

                    case OperatorType.DIVIDE:
                        return left.Value / right.Value;

                    default:
                        throw new Exception($"invalid operator {op.OpType}");
                }
            }

            #endregion private methods

            #region Methods

            public Calculator(string expr)
            {
                Expression = expr;
            }

            public decimal calculate()
            {
                parse();
                return ArgList[0].Value;
            }

            #endregion Methods
        }

        internal class Argument
        {
            #region Field

            public ArgType Type
            {
                get; set;
            } = ArgType.INVALID;

            //valid for  operator type
            public OperatorType OpType
            {
                get; set;
            } = OperatorType.INVALID;

            //valid for number type
            public decimal Value { get; set; } = 0;

            #endregion Field

            #region Method

            //extract one token from input
            public void Assign(char c)
            {
                switch (Type)
                {
                    case ArgType.BRACKET_BEGIN:
                    case ArgType.BRACKET_END:
                        break;

                    case ArgType.OP:
                        {
                            OpType = Calculator.Operators.GetValueOrDefault(c);
                        }
                        break;

                    case ArgType.NUMBER:
                        {
                            Value = 10 * Value + decimal.Parse(c + "");
                        }

                        break;

                    default:
                        throw new ArgumentException($"invalid arg {c}");
                }
            }

            public void Ensure(ArgType type)
            {
                if (Type != type)
                {
                    throw new Exception($"Expected {type}, but was {Type}");
                }
            }

            #endregion Method
        }

        #endregion Calculator
    }
}