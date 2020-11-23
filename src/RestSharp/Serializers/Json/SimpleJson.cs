using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using RestSharp.Serializers.Json.SimpleJson;

// ReSharper disable once CheckNamespace

namespace RestSharp {
    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes JsonArray(IList&lt;object>) and JsonObject(IDictionary&lt;string,object>).
    /// All numbers are parsed to doubles.
    /// </summary>
    [GeneratedCode("simple-json", "1.0.0")]
    static class SimpleJson {
        const int TOKEN_NONE          = 0;
        const int TOKEN_CURLY_OPEN    = 1;
        const int TOKEN_CURLY_CLOSE   = 2;
        const int TOKEN_SQUARED_OPEN  = 3;
        const int TOKEN_SQUARED_CLOSE = 4;
        const int TOKEN_COLON         = 5;
        const int TOKEN_COMMA         = 6;
        const int TOKEN_STRING        = 7;
        const int TOKEN_NUMBER        = 8;
        const int TOKEN_TRUE          = 9;
        const int TOKEN_FALSE         = 10;
        const int TOKEN_NULL          = 11;
        const int BUILDER_CAPACITY    = 2000;

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An IList&lt;object>, a IDictionary&lt;string,object>, a double, a string, null, true, or false</returns>
        public static object? DeserializeObject(string json) {
            if (TryDeserializeObject(json, out var obj)) return obj;

            throw new SerializationException("Invalid JSON string");
        }

        static bool TryDeserializeObject(char[]? json, out object? obj) {
            var success = true;

            if (json != null) {
                var index = 0;
                obj = ParseValue(json, ref index, ref success);
            }
            else
                obj = null;

            return success;
        }

        static bool TryDeserializeObject(string json, out object? obj) => TryDeserializeObject(json.ToCharArray(), out obj);

        /// <summary>
        /// Converts a IDictionary&lt;string,object> / IList&lt;object> object into a JSON string
        /// </summary>
        /// <param name="json">A IDictionary&lt;string,object> / IList&lt;object></param>
        /// <param name="jsonSerializerStrategy">Serializer strategy to use</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        static string? SerializeObject(object json, IJsonSerializerStrategy jsonSerializerStrategy) {
            var builder = new StringBuilder(BUILDER_CAPACITY);
            var success = SerializeValue(jsonSerializerStrategy, json, builder);
            return (success ? builder.ToString() : null);
        }

        public static string? SerializeObject(object json) => SerializeObject(json, CurrentJsonSerializerStrategy);

        public static string EscapeToJavascriptString(string jsonString) {
            if (string.IsNullOrEmpty(jsonString)) return jsonString;

            var sb = new StringBuilder();

            for (var i = 0; i < jsonString.Length;) {
                var c = jsonString[i++];

                if (c == '\\') {
                    var remainingLength = jsonString.Length - i;

                    if (remainingLength < 2) continue;

                    var lookahead = jsonString[i];

                    var append = lookahead switch {
                        '\\' => '\\',
                        '"'  => '\"',
                        't'  => '\t',
                        'b'  => '\b',
                        'n'  => '\n',
                        'r'  => '\r',
                        _    => char.MinValue
                    };

                    if (append != char.MinValue) {
                        sb.Append(append);
                        ++i;
                    }
                }
                else {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        static IDictionary<string, object?>? ParseObject(char[] json, ref int index, ref bool success) {
            var table = new JsonObject();

            // {
            NextToken(json, ref index);

            while (true) {
                var token = LookAhead(json, index);

                if (token == TOKEN_NONE) {
                    success = false;
                    return null;
                }
                else if (token == TOKEN_COMMA)
                    NextToken(json, ref index);
                else if (token == TOKEN_CURLY_CLOSE) {
                    NextToken(json, ref index);
                    return table;
                }
                else {
                    // name
                    var name = ParseString(json, ref index, ref success);

                    if (!success || name == null) {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken(json, ref index);

                    if (token != TOKEN_COLON) {
                        success = false;
                        return null;
                    }

                    // value
                    var value = ParseValue(json, ref index, ref success);

                    if (!success) {
                        success = false;
                        return null;
                    }

                    table[name] = value;
                }
            }
        }

        static JsonArray? ParseArray(char[] json, ref int index, ref bool success) {
            var array = new JsonArray();

            // [
            NextToken(json, ref index);

            while (true) {
                int token = LookAhead(json, index);

                if (token == TOKEN_NONE) {
                    success = false;
                    return null;
                }
                else if (token == TOKEN_COMMA)
                    NextToken(json, ref index);
                else if (token == TOKEN_SQUARED_CLOSE) {
                    NextToken(json, ref index);
                    break;
                }
                else {
                    var value = ParseValue(json, ref index, ref success);
                    if (!success) return null;

                    array.Add(value);
                }
            }

            return array;
        }

        static object? ParseValue(char[] json, ref int index, ref bool success) {
            switch (LookAhead(json, index)) {
                case TOKEN_STRING:       return ParseString(json, ref index, ref success);
                case TOKEN_NUMBER:       return ParseNumber(json, ref index, ref success);
                case TOKEN_CURLY_OPEN:   return ParseObject(json, ref index, ref success);
                case TOKEN_SQUARED_OPEN: return ParseArray(json, ref index, ref success);
                case TOKEN_TRUE:
                    NextToken(json, ref index);
                    return true;
                case TOKEN_FALSE:
                    NextToken(json, ref index);
                    return false;
                case TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case TOKEN_NONE: break;
            }

            success = false;
            return null;
        }

        static string? ParseString(char[] json, ref int index, ref bool success) {
            var s = new StringBuilder(BUILDER_CAPACITY);

            EatWhitespace(json, ref index);

            // "
            var  c        = json[index++];
            var complete = false;

            while (true) {
                if (index == json.Length) break;

                c = json[index++];

                if (c == '"') {
                    complete = true;
                    break;
                }

                if (c == '\\') {
                    if (index == json.Length) break;

                    c = json[index++];

                    if (c == '"')
                        s.Append('"');
                    else if (c == '\\')
                        s.Append('\\');
                    else if (c == '/')
                        s.Append('/');
                    else if (c == 'b')
                        s.Append('\b');
                    else if (c == 'f')
                        s.Append('\f');
                    else if (c == 'n')
                        s.Append('\n');
                    else if (c == 'r')
                        s.Append('\r');
                    else if (c == 't')
                        s.Append('\t');
                    else if (c == 'u') {
                        var remainingLength = json.Length - index;

                        if (remainingLength >= 4) {
                            // parse the 32 bit hex into an integer codepoint

                            if (!(success = UInt32.TryParse(
                                new string(json, index, 4),
                                NumberStyles.HexNumber,
                                CultureInfo.InvariantCulture,
                                out var codePoint
                            )))
                                return "";

                            // convert the integer codepoint to a unicode char and add to string
                            if (0xD800 <= codePoint && codePoint <= 0xDBFF) // if high surrogate
                            {
                                index           += 4; // skip 4 chars
                                remainingLength =  json.Length - index;

                                if (remainingLength >= 6) {
                                    if (new string(json, index, 2) == "\\u" && uint.TryParse(
                                        new string(json, index + 2, 4),
                                        NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture,
                                        out var lowCodePoint
                                    )) {
                                        if (0xDC00 <= lowCodePoint && lowCodePoint <= 0xDFFF) // if low surrogate
                                        {
                                            s.Append((char) codePoint);
                                            s.Append((char) lowCodePoint);
                                            index += 6; // skip 6 chars
                                            continue;
                                        }
                                    }
                                }

                                success = false; // invalid surrogate pair
                                return "";
                            }

                            s.Append(ConvertFromUtf32((int) codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                            break;
                    }
                }
                else
                    s.Append(c);
            }

            if (!complete) {
                success = false;
                return null;
            }

            return s.ToString();
        }

        static string ConvertFromUtf32(int utf32) {
            switch (utf32) {
                // http://www.java2s.com/Open-Source/CSharp/2.6.4-mono-.net-core/System/System/Char.cs.htm
                case < 0:
                case > 0x10FFFF:
                    throw new ArgumentOutOfRangeException(nameof(utf32), "The argument must be from 0 to 0x10FFFF.");
                case <= 0xD800 and <= 0xDFFF: throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
                case < 0x10000:               return new string((char) utf32, 1);
                default:
                    utf32 -= 0x10000;
                    return new string(new[] {(char) ((utf32 >> 10) + 0xD800), (char) (utf32 % 0x0400 + 0xDC00)});
            }
        }

        static object ParseNumber(char[] json, ref int index, ref bool success) {
            EatWhitespace(json, ref index);
            var    lastIndex  = GetLastIndexOfNumber(json, index);
            var    charLength = (lastIndex - index) + 1;
            object returnNumber;
            var    str = new string(json, index, charLength);

            if (str.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || str.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1) {
                success      = double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
                returnNumber = number;
            }
            else {
                success      = long.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
                returnNumber = number;
            }

            index = lastIndex + 1;
            return returnNumber;
        }

        static int GetLastIndexOfNumber(char[] json, int index) {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++) {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1) break;
            }

            return lastIndex - 1;
        }

        static void EatWhitespace(char[] json, ref int index) {
            for (; index < json.Length; index++) {
                if (" \t\n\r\b\f".IndexOf(json[index]) == -1) break;
            }
        }

        static int LookAhead(char[] json, int index) {
            var saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        static int NextToken(char[] json, ref int index) {
            EatWhitespace(json, ref index);
            if (index == json.Length) return TOKEN_NONE;

            var c = json[index];
            index++;

            switch (c) {
                case '{': return TOKEN_CURLY_OPEN;
                case '}': return TOKEN_CURLY_CLOSE;
                case '[': return TOKEN_SQUARED_OPEN;
                case ']': return TOKEN_SQUARED_CLOSE;
                case ',': return TOKEN_COMMA;
                case '"': return TOKEN_STRING;
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
                case '-':
                    return TOKEN_NUMBER;
                case ':': return TOKEN_COLON;
            }

            index--;
            var remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5) {
                if (json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e') {
                    index += 5;
                    return TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4) {
                if (json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e') {
                    index += 4;
                    return TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4) {
                if (json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l') {
                    index += 4;
                    return TOKEN_NULL;
                }
            }

            return TOKEN_NONE;
        }

        static bool SerializeValue(IJsonSerializerStrategy jsonSerializerStrategy, object? value, StringBuilder builder) {
            var success = true;

            switch (value) {
                case string stringValue:
                    success = SerializeString(stringValue, builder);
                    break;
                case IDictionary<string, object> dict:
                    success = SerializeObject(jsonSerializerStrategy, dict.Keys, dict.Values, builder);
                    break;
                case IDictionary<string, string> stringDictionary:
                    success = SerializeObject(jsonSerializerStrategy, stringDictionary.Keys, stringDictionary.Values, builder);
                    break;
                case null:
                    builder.Append("null");
                    break;
                case IEnumerable enumerableValue:
                    success = SerializeArray(jsonSerializerStrategy, enumerableValue, builder);
                    break;
                default: {
                    if (IsNumeric(value))
                        success = SerializeNumber(value, builder);
                    else if (value is bool b)
                        builder.Append(b ? "true" : "false");
                    else {
                        success = jsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out var serializedObject);
                        if (success) SerializeValue(jsonSerializerStrategy, serializedObject, builder);
                    }

                    break;
                }
            }

            return success;
        }

        static bool SerializeObject(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder) {
            builder.Append("{");
            var ke    = keys.GetEnumerator();
            var ve    = values.GetEnumerator();
            var first = true;

            while (ke.MoveNext() && ve.MoveNext()) {
                var key   = ke.Current;
                var value = ve.Current;
                if (!first) builder.Append(",");

                if (key is string stringKey)
                    SerializeString(stringKey, builder);
                else if (!SerializeValue(jsonSerializerStrategy, value, builder)) return false;

                builder.Append(":");
                if (!SerializeValue(jsonSerializerStrategy, value, builder)) return false;

                first = false;
            }

            builder.Append("}");
            return true;
        }

        static bool SerializeArray(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable anArray, StringBuilder builder) {
            builder.Append("[");
            var first = true;

            foreach (object value in anArray) {
                if (!first) builder.Append(",");
                if (!SerializeValue(jsonSerializerStrategy, value, builder)) return false;

                first = false;
            }

            builder.Append("]");
            return true;
        }

        static bool SerializeString(string aString, StringBuilder builder) {
            builder.Append("\"");
            var charArray = aString.ToCharArray();

            foreach (var c in charArray) {
                switch (c) {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            builder.Append("\"");
            return true;
        }

        static bool SerializeNumber(object number, StringBuilder builder) {
            switch (number) {
                case long l:
                    builder.Append(l.ToString(CultureInfo.InvariantCulture));
                    break;
                case ulong number1:
                    builder.Append(number1.ToString(CultureInfo.InvariantCulture));
                    break;
                case int i:
                    builder.Append(i.ToString(CultureInfo.InvariantCulture));
                    break;
                case uint u:
                    builder.Append(u.ToString(CultureInfo.InvariantCulture));
                    break;
                case decimal number1:
                    builder.Append(number1.ToString(CultureInfo.InvariantCulture));
                    break;
                case float f:
                    builder.Append(f.ToString(CultureInfo.InvariantCulture));
                    break;
                default:
                    builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
                    break;
            }

            return true;
        }

        static bool IsNumeric(object value) {
            switch (value) {
                case sbyte:
                case byte:
                case short:
                case ushort:
                case int:
                case uint:
                case long:
                case ulong:
                case float:
                case double:
                case decimal:
                    return true;
                default: return false;
            }
        }

        static IJsonSerializerStrategy? currentJsonSerializerStrategy;
        static IJsonSerializerStrategy CurrentJsonSerializerStrategy => currentJsonSerializerStrategy ??= PocoJsonSerializerStrategy;

        static PocoJsonSerializerStrategy? pocoJsonSerializerStrategy;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        static PocoJsonSerializerStrategy PocoJsonSerializerStrategy => pocoJsonSerializerStrategy ??= new PocoJsonSerializerStrategy();
    }
}
