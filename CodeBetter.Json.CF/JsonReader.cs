namespace CodeBetter.Json
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Helpers;

    public class JsonReader : IDisposable
    {
        private readonly TextReader _reader;
        private bool _disposed;
        public JsonReader(TextReader input)
        {
            _reader = input;
        }
        public JsonReader(Stream input) : this(new StreamReader(input, Encoding.UTF8)) { }        
        public JsonReader(string input) : this(new StringReader(input)) { }

        public virtual void SkipWhiteSpaces()
        {            
            while(true)
            {
                var c = Peek();
                if (!char.IsWhiteSpace(c))
                {
                    break;
                }
                _reader.Read();
            }
        }
        public virtual int? ReadInt32(bool isNullable)
        {
            var value = ReadNumericValue();
            return value == null ? (isNullable ? null : (int?)0) : Convert.ToInt32(value);
        }
        public virtual string ReadString()
        {
            if (Peek() != JsonTokens.StringDelimiter)
            {
                AssertAndConsumeNull();
                return null;   
            }
            Read(); //we know this is a StringDelimiter      
            var sb = new StringBuilder(25);
            var isEscaped = false;

            while(true)
            {
                var c = Read();
                if (c == '\\' && !isEscaped)
                {
                    isEscaped = true;
                    continue;
                }
                if (isEscaped)
                {                    
                    if (c == 'u') { sb.Append(HandleEscapedSequence()); }
                    else { sb.Append(FromEscaped(c)); }                    
                    isEscaped = false;
                    continue;
                }
                if (c == '"')
                {                 
                    break;
                }
                sb.Append(c);
            }            
            var str = sb.ToString();
            return str == "null" ? null : str;
        }
        public virtual double? ReadDouble(bool isNullable)
        {
            var value = ReadNumericValue();
            return value == null ? (isNullable ? null : (double?)0) : Convert.ToDouble(value);
        }
        public virtual DateTime? ReadDateTime(bool isNullable)
        {
            var seconds = ReadInt32(true);
            return seconds == null ? (isNullable) ? null : (DateTime?) DateTime.MinValue : DateHelper.FromUnixTime(seconds.Value);
        }
        public virtual char ReadChar()
        {
            var str = ReadString();            
            if (str == null)
            {
                return (char) 0;
            }
            if (str.Length > 1)
            {
                throw new JsonException("Expecting a character, but got a string");
            }                        
            return str[0];
        }
        public virtual object ReadEnum(Type type)
        {
            return Enum.Parse(type, ReadInt64(false).ToString(), false);
        }
        public virtual long? ReadInt64(bool isNullable)
        {
            var value = ReadNumericValue();
            return value == null ? (isNullable ? null : (long?)0) : Convert.ToInt64(value);
        }
        public virtual bool? ReadBool(bool isNullable)
        {
            var str = ReadNonStringValue('0');
            if (str == null) return isNullable ? null : (bool?) false;
            if (str.Equals("true")) return true;
            if (str.Equals("false")) return false;
            throw new JsonException("Expecting true or false, but got " + str);
        }

        public virtual float? ReadFloat(bool isNullable)
        {
            var value = ReadNumericValue();
            return value == null ? (isNullable ? null : (float?)0) : Convert.ToSingle(value);
        }
        public virtual decimal? ReadDecimal(bool isNullable)
        {
            var value = ReadNumericValue();
            return value == null ? (isNullable ? null : (decimal?)0) : Convert.ToDecimal(value);
        }
        public virtual short? ReadInt16(bool isNullable)
        {
            var value = ReadNumericValue();
            return value == null ? (isNullable ? null : (short?)0) : Convert.ToInt16(value);
        }
        public virtual string ReadNumericValue()
        {
            return ReadNonStringValue('0');
        }
        public virtual string ReadNonStringValue(char offset)
        {
            var sb = new StringBuilder(10);
            while (true)
            {
                var c = Peek();
                if (IsDelimiter(c))
                {
                    break;
                }
                var read = _reader.Read();
                if (read >= '0' && read <= '9')
                {
                    sb.Append(read - offset);
                }
                else
                {
                    sb.Append((char) read);
                }                
            }
            var str = sb.ToString();
            return str == "null" ? null : str;
        }
        public virtual bool IsDelimiter(char c)
        {
            return (c == JsonTokens.EndObjectLiteralCharacter || c == JsonTokens.EndArrayCharacter || c == JsonTokens.ElementSeparator || IsWhiteSpace(c));
        }
        public virtual bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c);
        }
        
        public virtual char Peek()
        {
            var c = _reader.Peek();
            return ValidateChar(c);
        }
        public virtual char Read()
        {
            var c = _reader.Read();
            return ValidateChar(c);
        }
        private static char ValidateChar(int c)
        {
            if (c == -1)
            {
                throw new JsonException("End of data");
            }
            return (char)c;
        }

        public virtual string FromEscaped(char c)
        {
            switch (c)
            {
                case '"':
                    return "\"";
                case '\\':
                    return "\\";
                case '/':
                    return "/";
                case 'b':
                    return "\b";
                case 'f':
                    return "\f";
                case 'r':
                    return "\r";
                case 'n':
                    return "\n";
                case 't':
                    return "\t";
                default:
                    throw new ArgumentException("Unrecognized escape character: " + c);
            }
        }
        protected internal virtual void AssertAndConsume(char character)
        {
            var c = Read();
            if (c != character)
            {
                throw new JsonException(string.Format("Expected character '{0}', but got: '{1}'", character, c));
            }
        }
        protected internal virtual void AssertAndConsumeNull()
        {
            if (Read() != 'n' || Read() != 'u' || Read() != 'l' || Read() != 'l')
            {
                throw new JsonException("Expected null");
            }            
        }
        protected internal bool AssertNextIsDelimiterOrSeparator(char endDelimiter)
        {
            var delimiter = Read();
            if (delimiter == endDelimiter)
            {
                return true;
            }
            if (delimiter == ',')
            {
                return false;                
            }
            throw new JsonException("Expected array separator or end of array, got: " + delimiter);            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _reader.Close();
            }
            _disposed = true;
        }

        private char HandleEscapedSequence()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 4; i++)
            {
                var c = Read();
                if (!IsHexDigit(c))
                {
                    throw new JsonException(string.Format("Expected hex digit but got: '{0}'", c));
                }
                sb.Append(c);
            }
            return (char)int.Parse(sb.ToString(), NumberStyles.HexNumber);             
        }
        private static bool IsHexDigit(char x)
        {
            return (x >= '0' && x <= '9') || (x >= 'a' && x <= 'f') || (x >= 'A' && x <= 'F');
        }
    }
}
