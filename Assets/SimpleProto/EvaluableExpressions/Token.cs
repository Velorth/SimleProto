using System;

namespace SimpleProto.Expressions
{
    /// <summary>
    /// Type of tokens
    /// </summary>
    public enum TokenType
    {
        None = -1,

        /// <summary>
        /// Real number
        /// </summary>
        Number,

        /// <summary>
        /// String in 'some text' format
        /// </summary>
        String,

        /// <summary>
        /// Boolean literal
        /// </summary>
        Boolean,

        /// <summary>
        /// Null literal
        /// </summary>
        Null,

        /// <summary>
        /// Left bracket
        /// </summary>
        LeftBracket,

        /// <summary>
        /// Right bracket
        /// </summary>
        RightBracket,

        /// <summary>
        /// Reference to table field in [columnName] format
        /// </summary>
        TableField,

        /// <summary>
        /// Binary operator
        /// </summary>
        Operator,

        /// <summary>
        /// Function or operator
        /// </summary>
        Function,

        /// <summary>
        /// Argument divider
        /// </summary>
        Comma,

        /// <summary>
        /// Date literal
        /// </summary>
        /// <example><![CDATA[#21/02/2016#]]></example>
        Date,

        /// <summary>
        /// Colon :
        /// </summary>
        Colon,

        /// <summary>
        /// Question mark
        /// </summary>
        QuestionMark
    }

    /// <summary>
    /// Expression token
    /// </summary>
    public sealed class Token
    {

        private bool _isLeftAssociative = true;
        public TokenType Type { get; set; }
        public string Text { get; set; }

        public bool IsLiteral
        {
            get
            {
                return Type == TokenType.Number 
                    || Type == TokenType.Boolean
                    || Type == TokenType.Null
                    || Type == TokenType.String 
                    || Type == TokenType.TableField
                    || Type == TokenType.Date;
            }
        }

        public bool IsFunction
        {
            get { return Type == TokenType.Function; }
        }

        public bool IsArgumentDivider { get { return Type == TokenType.Comma; } }

        public bool IsLeftAssociative
        {
            get { return _isLeftAssociative; }
            set { _isLeftAssociative = value; }
        }

        public int Precedence { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: '{1}'", Type, Text);
        }
    }
}