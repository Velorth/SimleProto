using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace SimpleProto.Expressions
{
    /// <summary>
    /// Parser class
    /// </summary>
    /// TODO: Use RegExp?
    public class ExpressionParser
    {
        private const string Digits = "0123456789";
        private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Operators = "+-*/=<>!";

        /// <summary>
        /// Process expression string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>List of tokens in expression</returns>
        /// <exception cref="ParsingException"></exception>
        public IList<Token> ParseExpression(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new Token[0];

            var result = new List<Token>();

            var startIndex = 0;
            var canBeLiteral = true;

            while (startIndex < text.Length)
            {
                var symbol = text[startIndex];

                if (canBeLiteral && (Digits.Contains(symbol)))
                {
                    var token = ParseNumber(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = false;
                }
                else if (canBeLiteral && symbol == '-')
                {
                    var token = ExpressionLibrary.FindOperator("~-").BuildToken();
                    result.Add(token);
                    startIndex++;
                }
                else if (symbol == '?')
                {
                    var token = new Token { Text = "?", Type = TokenType.QuestionMark, IsLeftAssociative = false};
                    result.Add(token);
                    canBeLiteral = true;
                    startIndex++;
                }
                else if (symbol == ':')
                {
                    var token = new Token { Text = ":", Type = TokenType.Colon };
                    result.Add(token);
                    canBeLiteral = true;
                    startIndex++;
                }
                else if (symbol == '[')
                {
                    var token = ParseField(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = false;
                }
                else if (symbol == ' ')
                {
                    startIndex++;
                }
                else if (symbol == '\'')
                {
                    var token = ParseString(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = false;
                }
                else if (IsBooleanLiteral(text, startIndex))
                {
                    var token = ParseBoolean(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = false;
                }
                else if (IsKeyWord("null", text, startIndex))
                {
                    var token = new Token {Text = "null", Type = TokenType.Null};
                    result.Add(token);
                    startIndex += 4;
                    canBeLiteral = false;
                }
                else if (Operators.Contains(symbol))
                {
                    var token = ParseOperator(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = true;
                }
                else if (Letters.Contains(symbol))
                {
                    var token = ParseIdentifier(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = token.Type == TokenType.Operator;
                }
                else if (symbol == '(')
                {
                    result.Add(new Token {Text = "(", Type = TokenType.LeftBracket});
                    canBeLiteral = true;
                    startIndex++;
                }
                else if (symbol == ')')
                {
                    result.Add(new Token { Text = ")", Type = TokenType.RightBracket });
                    canBeLiteral = false;
                    startIndex++;
                }
                else if (symbol == ',')
                {
                    result.Add(new Token { Text = ",", Type = TokenType.Comma });
                    canBeLiteral = true;
                    startIndex++;
                }
                else if (symbol == '#')
                {
                    var token = ParseDate(text, ref startIndex);
                    result.Add(token);
                    canBeLiteral = false;
                }
                else
                {
                    throw new ParsingException(string.Format("Unexpected character '{0}'", symbol));
                }
            }
            return result;
        }

        private Token ParseDate(string text, ref int startIndex)
        {
            if (startIndex == text.Length - 1)
                throw new ParsingException("Unclosed date literal. '#' expected");
            var endIndex = text.IndexOf('#', startIndex + 1);
            if (endIndex == -1)
                throw new ParsingException("Unclosed date literal. '#' expected");
            var token = new Token
            {
                Text = text.Substring(startIndex, endIndex-startIndex + 1),
                Type = TokenType.Date
            };
            startIndex = endIndex + 1;

            return token;
        }

        private Token ParseIdentifier(string text, ref int startIndex)
        {
            var nextIndex = startIndex;
            while (nextIndex < text.Length && (Letters.Contains(text[nextIndex]) || Digits.Contains(text[nextIndex]) || text[nextIndex] == '_'))
            {
                nextIndex++;
            }
            
            var tokenText = text.Substring(startIndex, nextIndex - startIndex);
            startIndex = nextIndex;

            var lowerText = tokenText.ToLowerInvariant();

            if (lowerText == "and")
            {
                return new Token
                {
                    Text = tokenText,
                    Type = TokenType.Operator,
                    Precedence = 13
                };
            }
            if (lowerText == "or")
            {
                return new Token
                {
                    Text = tokenText,
                    Type = TokenType.Operator,
                    Precedence = 14
                };
            }

            var result = new Token
            {
                Text = tokenText,
                Type = TokenType.Function
            };
            return result;
        }

        private void AddAstNode(Stack<AstNode> stack, Token token, int arity = 2)
        {
            var node = new AstNode();
            node.Token = token;

            for (var i = 0; i < arity; ++i)
            {
                node.Children.Insert(0, stack.Pop());
            }

            stack.Push(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>Root node of syntax tree or null in case of empty expression</returns>
        /// <exception cref="ParsingException">in case of grammar errors</exception>
        public AstNode BuildAst(IList<Token> tokens)
        {
            if (tokens.Count == 0)
                return null;
            
            var stack = new Stack<Token>();
            var astStack = new Stack<AstNode>();
            var arityStack = new Stack<int>();

            TokenType prevTokenType = TokenType.None;
            foreach (var token in tokens)
            {
                if (token.IsLiteral)
                {
                    astStack.Push(new AstNode
                    {
                        Token = token
                    });
                }
                else if (token.Type == TokenType.Colon)
                {
                    while (stack.Count > 0 && stack.Peek().Type != TokenType.QuestionMark)
                    {
                        var @operator = stack.Pop();
                        AddAstNode(astStack, @operator, arityStack.Pop());
                    }

                    stack.Pop();
                    var firstParam = astStack.Pop();

                    var ternaryToken = ExpressionLibrary.FindOperator("?:").BuildToken();

                    while (stack.Count > 0 && stack.Peek().Type == TokenType.Operator &&
                        (ternaryToken.IsLeftAssociative && ternaryToken.Precedence >= stack.Peek().Precedence ||
                        !ternaryToken.IsLeftAssociative && ternaryToken.Precedence > stack.Peek().Precedence))
                    {
                        var top = stack.Pop();
                        AddAstNode(astStack, top, arityStack.Pop());
                    }

                    astStack.Push(firstParam);
                    
                    arityStack.Push(3);
                    stack.Push(ternaryToken);
                }
                else if (token.Type == TokenType.QuestionMark)
                {
                    stack.Push(token);
                }
                else if (token.Type == TokenType.Colon)
                {

                    stack.Push(token);
                }
                else if (token.IsFunction)
                {
                    stack.Push(token);
                    arityStack.Push(1);
                }
                else if (token.IsArgumentDivider)
                {
                    if (stack.Count == 0 || stack.Peek().Type != TokenType.LeftBracket)
                    {
                        throw new ParsingException("'(' Expected");
                    }
                    var arity = arityStack.Pop() + 1;
                    arityStack.Push(arity);
                }
                else if (token.Type == TokenType.Operator)
                {
                    while (stack.Count > 0 && stack.Peek().Type == TokenType.Operator &&
                        (token.IsLeftAssociative && token.Precedence >= stack.Peek().Precedence ||
                        !token.IsLeftAssociative && token.Precedence > stack.Peek().Precedence))
                    {
                        var top = stack.Pop();
                        AddAstNode(astStack, top, arityStack.Pop());
                    }
                    var operatorInfo = ExpressionLibrary.FindOperator(token.Text);
                    arityStack.Push(operatorInfo.Arity);
                    stack.Push(token);
                }
                else if (token.Type == TokenType.LeftBracket)
                {
                    stack.Push(token);
                }
                else if (token.Type == TokenType.RightBracket)
                {
                    // Hack for zero parameter functions
                    if (prevTokenType == TokenType.LeftBracket)
                    {
                        arityStack.Pop();
                        arityStack.Push(0);
                    }

                    while (stack.Count > 0 && stack.Peek().Type != TokenType.LeftBracket)
                    {
                        var @operator = stack.Pop();
                        AddAstNode(astStack, @operator, arityStack.Pop());
                    }

                    if (stack.Count == 0)
                        throw new ParsingException("'(' expected");

                    stack.Pop();

                    if (stack.Count > 0 && stack.Peek().Type == TokenType.Function)
                    {
                        var function = stack.Pop();
                        AddAstNode(astStack, function, arityStack.Pop());
                    }
                }

                prevTokenType = token.Type;
            }

            while (stack.Count > 0)
            {
                var token = stack.Pop();
                AddAstNode(astStack, token, arityStack.Pop());

                if (token.Type == TokenType.LeftBracket)
                    throw new ParsingException("')' expected");
            }

            return astStack.Pop();
        }

        private Token ParseOperator(string text, ref int startIndex)
        {
            Token token;
            if (startIndex + 1 == text.Length)
            {
                token = new Token {Text = text.Substring(startIndex, 1), Type = TokenType.Operator};
                startIndex ++;
            }
            else if (text[startIndex] == '<' && text[startIndex + 1] == '=' ||
                     text[startIndex] == '>' && text[startIndex + 1] == '=' ||
                     text[startIndex] == '<' && text[startIndex + 1] == '>' ||
                     text[startIndex] == '!' && text[startIndex + 1] == '=')
            {
                token = new Token { Text = text.Substring(startIndex, 2), Type = TokenType.Operator };
                startIndex += 2;
            }
            else
            {
                token = new Token { Text = text.Substring(startIndex, 1), Type = TokenType.Operator };
                startIndex++;
            }
            
            token.Precedence = GetOperatorPrecedence(token.Text);

            return token;
        }

        private int GetOperatorPrecedence(string text)
        {
            var operatorInfo = ExpressionLibrary.FindOperator(text);
            if (operatorInfo == null)
                throw new ParsingException(String.Format("Unknown operator '{0}'", text));
            return operatorInfo.Precedence;
        }

        private Token ParseField(string text, ref int startIndex)
        {
            var closingIndex = text.IndexOf(']', startIndex);
            if (closingIndex == -1)
                throw new ParsingException("Expected \']\' not found");

            var result = new Token
            {
                Text = text.Substring(startIndex, closingIndex - startIndex + 1),
                Type = TokenType.TableField
            };

            startIndex = closingIndex + 1;

            return result;
        }

        private Token ParseNumber(string text, ref int startIndex)
        {
            var endIndex = startIndex + 1;
            while (endIndex < text.Length && (Digits.Contains(text[endIndex]) || text[endIndex] == '.'))
            {
                endIndex++;
            }
            var result = new Token
            {
                Text = text.Substring(startIndex, endIndex - startIndex),
                Type = TokenType.Number
            };
            startIndex = endIndex;

            return result;
        }

        private bool IsKeyWord([NotNull]string keyword, [NotNull]string text, int startIndex)
        {
            var nextIndex = startIndex + keyword.Length;
            if (nextIndex > text.Length)
            {
                return false;
            }
            if (nextIndex < text.Length &&
                (text[nextIndex] == '_' ||
                Digits.Contains(text[nextIndex]) ||
                Letters.Contains(text[nextIndex])))
            {
                return false;
            }
            var sub = text.Substring(startIndex).ToLowerInvariant();
            return sub.StartsWith(keyword);
        }

        private bool IsBooleanLiteral(string text, int startIndex)
        {
            return IsKeyWord("true", text, startIndex) || IsKeyWord("false", text, startIndex);
        }

        private Token ParseBoolean(string text, ref int startIndex)
        {
            var lower = text.ToLower();
            int length = lower.Substring(startIndex).StartsWith("true") ? 4 : 5;
            var token = new Token
                {
                    Type = TokenType.Boolean,
                    Text = text.Substring(startIndex, length)
                };
            startIndex += length;
            return token;
        }

        private Token ParseString(string text, ref int startIndex)
        {
            var closingIndex = text.IndexOf('\'', startIndex + 1);
            if (closingIndex == -1)
                throw new ParsingException("String is not complete ''' is required.");

            var result = new Token
            {
                Text = text.Substring(startIndex, closingIndex - startIndex + 1),
                Type = TokenType.String
            };

            startIndex = closingIndex + 1;
            return result;
        }

        public IEvaluable BuildExpression(string expression)
        {
            var infix = ParseExpression(expression);
            var ast = BuildAst(infix);
            return BuildExpression(ast);
        }

        /// <summary>
        /// Builds evaluable tree from abstract syntax tree
        /// </summary>
        /// <param name="ast">Syntax tree</param>
        /// <returns></returns>
        public IEvaluable BuildExpression(AstNode ast)
        {
            if (ast == null)
                return new ConstNode
                {
                    Value = null
                };

            switch (ast.TokenType)
            {
                case TokenType.Number:
                    return new ConstNode { Value = EvaluateNumber(ast.Text) };
                case TokenType.String:
                    return new ConstNode { Value = EvaluateString(ast.Text)};
                case TokenType.Date:
                    return new ConstNode { Value = EvaluateDate(ast.Text) };
                case TokenType.Boolean:
                    return new ConstNode {Value = EvaluateBoolean(ast.Text)};
                case TokenType.Null:
                    return new ConstNode {Value = null};
                case TokenType.Operator:
                    var operatorInfo = ExpressionLibrary.FindOperator(ast.Text);
                    var operatorNode = new OperatorNode {Operator = operatorInfo};
                    // ReSharper disable once RedundantTypeArgumentsOfMethod
                    foreach (var child in ast.Children.Select<AstNode, IEvaluable>(BuildExpression))
                        operatorNode.Children.Add(child);

                    return operatorNode;
                case TokenType.TableField:
                    return new FieldNode {FieldName = ast.Text.Substring(1, ast.Text.Length - 2)};
                case TokenType.Function:
                    var functionInfo = ExpressionLibrary.FindFunction(ast.Text);
                    var functionNode = new FunctionNode { Function = functionInfo };
                    foreach (var child in ast.Children.Select<AstNode, IEvaluable>(BuildExpression))
                        functionNode.Children.Add(child);
                    return functionNode;
                default:
                    throw new NotSupportedException(string.Format("Token '{0}' of type {1} is not supported", ast.Text,
                        ast.TokenType));
            }
        }

        private object EvaluateBoolean(string tokenText)
        {
            return tokenText.ToLowerInvariant() == "true";
        }

        private object EvaluateString(string tokenText)
        {
            if (string.IsNullOrEmpty(tokenText))
                return string.Empty;

            if (tokenText[0] != '\'')
                return tokenText;

            return tokenText.Substring(1, tokenText.Length - 2);
        }

        private object EvaluateDate(string tokenText)
        {
            try
            {
                return DateTime.Parse(tokenText.Substring(1, tokenText.Length - 2));
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        private object EvaluateNumber(string tokenText)
        {
            try
            {
                return Convert.ToSingle(tokenText, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return float.NaN;
            }
        }
    }
}