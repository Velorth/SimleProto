using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleProto.Expressions
{
    public sealed class AstNode
    {
        private readonly List<AstNode> _children = new List<AstNode>();

        public TokenType TokenType
        {
            get
            {
                return Token == null
                    ? TokenType.None
                    : Token.Type;
            }
        }

        public int Precedence { get { return Token == null? 0 : Token.Precedence; } }
        public string Text { get { return Token == null ? "" : Token.Text; } }
        public Token Token { get; set; }
        public List<AstNode> Children { get { return _children; } }

        public string BuildExpressionString()
        {
            switch (TokenType)
            {
                case TokenType.None:
                    return "";
                case TokenType.Number:
                case TokenType.String:
                case TokenType.TableField:
                case TokenType.Date:
                    return Token.Text;
                case TokenType.Operator:
                    var left = Children[0].BuildExpressionString();
                    var right = Children[1].BuildExpressionString();
                    if (Children[0].Token.Type == TokenType.Operator && Children[0].Precedence > Precedence)
                        left = string.Format("({0})", left);
                    if (Children[1].Token.Type == TokenType.Operator && Children[1].Precedence > Precedence)
                        right = string.Format("({0})", right);
                    return string.Format("{0} {1} {2}", left, Token.Text, right);
                case TokenType.Function:
                    var builder = new StringBuilder();
                    builder.Append(Token.Text);
                    builder.Append("(");
                    for (var i = 0; i < Children.Count; ++i)
                    {
                        builder.Append(Children[i].BuildExpressionString());
                        if (Children.Count - 1 != i)
                            builder.Append(", ");
                    }
                    builder.Append(")");
                    return builder.ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}