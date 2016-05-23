namespace UnityEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    internal static class Json
    {
        public static object Deserialize(string json)
        {
            if (json == null)
            {
                return null;
            }
            return Parser.Parse(json);
        }

        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        private sealed class Parser : IDisposable
        {
            private StringReader json;
            private const string WORD_BREAK = "{}[],:\"";

            private Parser(string jsonString)
            {
                this.json = new StringReader(jsonString);
            }

            public void Dispose()
            {
                this.json.Dispose();
                this.json = null;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(this.PeekChar))
                {
                    this.json.Read();
                    if (this.json.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            public static bool IsWordBreak(char c)
            {
                if (!char.IsWhiteSpace(c))
                {
                    return ("{}[],:\"".IndexOf(c) != -1);
                }
                return true;
            }

            public static object Parse(string jsonString)
            {
                using (Json.Parser parser = new Json.Parser(jsonString))
                {
                    return parser.ParseValue();
                }
            }

            private List<object> ParseArray()
            {
                List<object> list = new List<object>();
                this.json.Read();
                bool flag = true;
                while (flag)
                {
                    object obj2;
                    TOKEN nextToken = this.NextToken;
                    if (nextToken != TOKEN.NONE)
                    {
                        if (nextToken == TOKEN.SQUARED_CLOSE)
                        {
                            goto Label_002C;
                        }
                        if (nextToken == TOKEN.COMMA)
                        {
                            continue;
                        }
                        goto Label_0030;
                    }
                    return null;
                Label_002C:
                    flag = false;
                    continue;
                Label_0030:
                    obj2 = this.ParseByToken(nextToken);
                    list.Add(obj2);
                }
                return list;
            }

            private object ParseByToken(TOKEN token)
            {
                switch (token)
                {
                    case TOKEN.CURLY_OPEN:
                        return this.ParseObject();

                    case TOKEN.SQUARED_OPEN:
                        return this.ParseArray();

                    case TOKEN.STRING:
                        return this.ParseString();

                    case TOKEN.NUMBER:
                        return this.ParseNumber();

                    case TOKEN.TRUE:
                        return true;

                    case TOKEN.FALSE:
                        return false;

                    case TOKEN.NULL:
                        return null;
                }
                return null;
            }

            private object ParseNumber()
            {
                double num;
                string nextWord = this.NextWord;
                if (nextWord.IndexOf('.') == -1)
                {
                    long num2;
                    long.TryParse(nextWord, NumberStyles.Any, CultureInfo.InvariantCulture, out num2);
                    return num2;
                }
                double.TryParse(nextWord, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
                return num;
            }

            private Dictionary<string, object> ParseObject()
            {
                TOKEN token;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                this.json.Read();
            Label_0012:
                token = this.NextToken;
                if (token != TOKEN.NONE)
                {
                    if (token == TOKEN.CURLY_CLOSE)
                    {
                        return dictionary;
                    }
                    if (token == TOKEN.COMMA)
                    {
                        goto Label_0012;
                    }
                }
                else
                {
                    return null;
                }
                string str = this.ParseString();
                if (str == null)
                {
                    return null;
                }
                if (this.NextToken != TOKEN.COLON)
                {
                    return null;
                }
                this.json.Read();
                dictionary[str] = this.ParseValue();
                goto Label_0012;
            }

            private string ParseString()
            {
                StringBuilder builder = new StringBuilder();
                this.json.Read();
                bool flag = true;
                while (flag)
                {
                    char[] chArray;
                    int num;
                    if (this.json.Peek() == -1)
                    {
                        flag = false;
                        break;
                    }
                    char nextChar = this.NextChar;
                    if (nextChar == '"')
                    {
                        flag = false;
                    }
                    else
                    {
                        if (nextChar != '\\')
                        {
                            goto Label_012F;
                        }
                        if (this.json.Peek() == -1)
                        {
                            flag = false;
                        }
                        else
                        {
                            nextChar = this.NextChar;
                            switch (nextChar)
                            {
                                case '"':
                                case '/':
                                case '\\':
                                    goto Label_00B7;

                                case 'r':
                                    builder.Append('\r');
                                    break;

                                case 't':
                                    builder.Append('\t');
                                    break;

                                case 'u':
                                    chArray = new char[4];
                                    num = 0;
                                    goto Label_0113;

                                case 'n':
                                    builder.Append('\n');
                                    break;

                                case 'b':
                                    goto Label_00C1;

                                case 'f':
                                    goto Label_00CB;
                            }
                        }
                    }
                    continue;
                Label_00B7:
                    builder.Append(nextChar);
                    continue;
                Label_00C1:
                    builder.Append('\b');
                    continue;
                Label_00CB:
                    builder.Append('\f');
                    continue;
                Label_0103:
                    chArray[num] = this.NextChar;
                    num++;
                Label_0113:
                    if (num < 4)
                    {
                        goto Label_0103;
                    }
                    builder.Append((char) Convert.ToInt32(new string(chArray), 0x10));
                    continue;
                Label_012F:
                    builder.Append(nextChar);
                }
                return builder.ToString();
            }

            private object ParseValue()
            {
                TOKEN nextToken = this.NextToken;
                return this.ParseByToken(nextToken);
            }

            private char NextChar
            {
                get
                {
                    return Convert.ToChar(this.json.Read());
                }
            }

            private TOKEN NextToken
            {
                get
                {
                    this.EatWhitespace();
                    if (this.json.Peek() == -1)
                    {
                        return TOKEN.NONE;
                    }
                    switch (this.PeekChar)
                    {
                        case ']':
                            this.json.Read();
                            return TOKEN.SQUARED_CLOSE;

                        case '{':
                            return TOKEN.CURLY_OPEN;

                        case '}':
                            this.json.Read();
                            return TOKEN.CURLY_CLOSE;

                        case '"':
                            return TOKEN.STRING;

                        case ',':
                            this.json.Read();
                            return TOKEN.COMMA;

                        case '-':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            return TOKEN.NUMBER;

                        case ':':
                            return TOKEN.COLON;

                        case '[':
                            return TOKEN.SQUARED_OPEN;
                    }
                    string nextWord = this.NextWord;
                    if (!(nextWord == "false"))
                    {
                        if (nextWord == "true")
                        {
                            return TOKEN.TRUE;
                        }
                        if (nextWord == "null")
                        {
                            return TOKEN.NULL;
                        }
                        return TOKEN.NONE;
                    }
                    return TOKEN.FALSE;
                }
            }

            private string NextWord
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    while (!IsWordBreak(this.PeekChar))
                    {
                        builder.Append(this.NextChar);
                        if (this.json.Peek() == -1)
                        {
                            break;
                        }
                    }
                    return builder.ToString();
                }
            }

            private char PeekChar
            {
                get
                {
                    return Convert.ToChar(this.json.Peek());
                }
            }

            private enum TOKEN
            {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            }
        }

        private sealed class Serializer
        {
            private StringBuilder builder = new StringBuilder();

            private Serializer()
            {
            }

            public static string Serialize(object obj)
            {
                Json.Serializer serializer1 = new Json.Serializer();
                serializer1.SerializeValue(obj);
                return serializer1.builder.ToString();
            }

            private void SerializeArray(IList anArray)
            {
                this.builder.Append('[');
                bool flag = true;
                foreach (object obj2 in anArray)
                {
                    if (!flag)
                    {
                        this.builder.Append(',');
                    }
                    this.SerializeValue(obj2);
                    flag = false;
                }
                this.builder.Append(']');
            }

            private void SerializeObject(IDictionary obj)
            {
                bool flag = true;
                this.builder.Append('{');
                foreach (object obj2 in obj.Keys)
                {
                    if (!flag)
                    {
                        this.builder.Append(',');
                    }
                    this.SerializeString(obj2.ToString());
                    this.builder.Append(':');
                    this.SerializeValue(obj[obj2]);
                    flag = false;
                }
                this.builder.Append('}');
            }

            private void SerializeOther(object value)
            {
                if (value is float)
                {
                    this.builder.Append(((float) value).ToString("R", CultureInfo.InvariantCulture));
                }
                else if ((((value is int) || (value is uint)) || ((value is long) || (value is sbyte))) || (((value is byte) || (value is short)) || ((value is ushort) || (value is ulong))))
                {
                    this.builder.Append(value);
                }
                else if ((value is double) || (value is decimal))
                {
                    this.builder.Append(Convert.ToDouble(value).ToString("R", CultureInfo.InvariantCulture));
                }
                else
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    foreach (FieldInfo info in value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToList<FieldInfo>())
                    {
                        dictionary.Add(info.Name, info.GetValue(value));
                    }
                    foreach (PropertyInfo info2 in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList<PropertyInfo>())
                    {
                        dictionary.Add(info2.Name, info2.GetValue(value, null));
                    }
                    this.SerializeObject(dictionary);
                }
            }

            private void SerializeString(string str)
            {
                this.builder.Append('"');
                foreach (char ch in str.ToCharArray())
                {
                    switch (ch)
                    {
                        case '\b':
                        {
                            this.builder.Append(@"\b");
                            continue;
                        }
                        case '\t':
                        {
                            this.builder.Append(@"\t");
                            continue;
                        }
                        case '\n':
                        {
                            this.builder.Append(@"\n");
                            continue;
                        }
                        case '\f':
                        {
                            this.builder.Append(@"\f");
                            continue;
                        }
                        case '\r':
                        {
                            this.builder.Append(@"\r");
                            continue;
                        }
                        case '"':
                        {
                            this.builder.Append("\\\"");
                            continue;
                        }
                        case '\\':
                        {
                            this.builder.Append(@"\\");
                            continue;
                        }
                    }
                    int num2 = Convert.ToInt32(ch);
                    if ((num2 >= 0x20) && (num2 <= 0x7e))
                    {
                        this.builder.Append(ch);
                    }
                    else
                    {
                        this.builder.Append(@"\u");
                        this.builder.Append(num2.ToString("x4"));
                    }
                }
                this.builder.Append('"');
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    this.builder.Append("null");
                }
                else
                {
                    string str = value as string;
                    if (str != null)
                    {
                        this.SerializeString(str);
                    }
                    else if (value is bool)
                    {
                        this.builder.Append(((bool) value) ? "true" : "false");
                    }
                    else
                    {
                        IList anArray = value as IList;
                        if (anArray != null)
                        {
                            this.SerializeArray(anArray);
                        }
                        else
                        {
                            IDictionary dictionary = value as IDictionary;
                            if (dictionary != null)
                            {
                                this.SerializeObject(dictionary);
                            }
                            else if (value is char)
                            {
                                this.SerializeString(new string((char) value, 1));
                            }
                            else
                            {
                                this.SerializeOther(value);
                            }
                        }
                    }
                }
            }
        }
    }
}

