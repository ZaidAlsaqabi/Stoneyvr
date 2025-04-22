/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Simplified version for our Stonehenge project
 * 
 * Features / attributes:
 * - provides a simple(!) and fast parser (minimal GC, no regex, very fast)
 * - simple and fast string builder
 * - supports null values
 * - supports all types (boolean, number, string, array, object)
 * - provides easy access to the data (with . or ["xxx"] notation)
 * - can serialize/deserialize into WWWForm's post data
 * - no external dependencies
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SimpleJSON
{
    public enum JSONNodeType
    {
        Array = 1,
        Object = 2,
        String = 3,
        Number = 4,
        Boolean = 5,
        NullValue = 6,
        None = 7
    }

    public abstract partial class JSONNode
    {
        #region common interface
        public virtual JSONNode this[int aIndex] { get { return null; } set { } }
        public virtual JSONNode this[string aKey] { get { return null; } set { } }
        public virtual string Value { get { return ""; } set { } }
        public virtual int Count { get { return 0; } }
        public virtual bool IsNumber { get { return false; } }
        public virtual bool IsString { get { return false; } }
        public virtual bool IsBoolean { get { return false; } }
        public virtual bool IsNull { get { return false; } }
        public virtual bool IsArray { get { return false; } }
        public virtual bool IsObject { get { return false; } }
        public virtual JSONNodeType Tag { get { return JSONNodeType.None; } }
        
        public virtual bool AsBool
        {
            get
            {
                bool v = false;
                if (bool.TryParse(Value, out v))
                    return v;
                return !string.IsNullOrEmpty(Value);
            }
            set { Value = (value) ? "true" : "false"; }
        }
        
        public virtual float AsFloat
        {
            get
            {
                float v = 0;
                if (float.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                    return v;
                return 0;
            }
            set { Value = value.ToString(CultureInfo.InvariantCulture); }
        }
        
        public virtual double AsDouble
        {
            get
            {
                double v = 0;
                if (double.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                    return v;
                return 0;
            }
            set { Value = value.ToString(CultureInfo.InvariantCulture); }
        }
        
        public virtual int AsInt
        {
            get
            {
                int v = 0;
                if (int.TryParse(Value, out v))
                    return v;
                return 0;
            }
            set { Value = value.ToString(); }
        }
        
        public virtual long AsLong
        {
            get
            {
                long v = 0;
                if (long.TryParse(Value, out v))
                    return v;
                return 0;
            }
            set { Value = value.ToString(); }
        }
        #endregion
    }

    public class JSONString : JSONNode
    {
        private string m_Data;

        public JSONString(string aData)
        {
            m_Data = aData;
        }

        public override JSONNodeType Tag { get { return JSONNodeType.String; } }
        public override bool IsString { get { return true; } }
        public override string Value { get { return m_Data; } set { m_Data = value; } }
    }

    public class JSONNumber : JSONNode
    {
        private double m_Data;

        public JSONNumber(double aData)
        {
            m_Data = aData;
        }

        public override JSONNodeType Tag { get { return JSONNodeType.Number; } }
        public override bool IsNumber { get { return true; } }
        public override string Value { get { return m_Data.ToString(CultureInfo.InvariantCulture); } set { double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out m_Data); } }
        public override double AsDouble { get { return m_Data; } set { m_Data = value; } }
        public override float AsFloat { get { return (float)m_Data; } set { m_Data = value; } }
        public override int AsInt { get { return (int)m_Data; } set { m_Data = value; } }
        public override long AsLong { get { return (long)m_Data; } set { m_Data = value; } }
    }

    public class JSONBool : JSONNode
    {
        private bool m_Data;

        public JSONBool(bool aData)
        {
            m_Data = aData;
        }

        public override JSONNodeType Tag { get { return JSONNodeType.Boolean; } }
        public override bool IsBoolean { get { return true; } }
        public override string Value { get { return m_Data ? "true" : "false"; } set { m_Data = (value.ToLower() == "true"); } }
        public override bool AsBool { get { return m_Data; } set { m_Data = value; } }
    }

    public class JSONNull : JSONNode
    {
        public override JSONNodeType Tag { get { return JSONNodeType.NullValue; } }
        public override bool IsNull { get { return true; } }
        public override string Value { get { return "null"; } set { } }
    }

    public class JSONArray : JSONNode, IEnumerable
    {
        private List<JSONNode> m_List = new List<JSONNode>();

        public override JSONNodeType Tag { get { return JSONNodeType.Array; } }
        public override bool IsArray { get { return true; } }
        public override JSONNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return null;
                return m_List[aIndex];
            }
            set
            {
                if (value == null)
                    value = new JSONNull();
                if (aIndex < 0 || aIndex >= m_List.Count)
                    m_List.Add(value);
                else
                    m_List[aIndex] = value;
            }
        }
        public override int Count { get { return m_List.Count; } }

        public IEnumerator GetEnumerator()
        {
            foreach (JSONNode N in m_List)
                yield return N;
        }
    }

    public class JSONObject : JSONNode, IEnumerable
    {
        private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

        public override JSONNodeType Tag { get { return JSONNodeType.Object; } }
        public override bool IsObject { get { return true; } }
        public override JSONNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                    return m_Dict[aKey];
                return null;
            }
            set
            {
                if (value == null)
                    value = new JSONNull();
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = value;
                else
                    m_Dict.Add(aKey, value);
            }
        }
        public override int Count { get { return m_Dict.Count; } }

        public IEnumerator GetEnumerator()
        {
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                yield return N;
        }
    }

    public static class JSON
    {
        public static JSONNode Parse(string aJSON)
        {
            Stack<JSONNode> stack = new Stack<JSONNode>();
            JSONNode ctx = null;
            int i = 0;
            string token = "";
            string tokenName = "";
            bool quoteMode = false;
            
            while (i < aJSON.Length)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (quoteMode)
                        {
                            token += aJSON[i];
                            break;
                        }
                        stack.Push(new JSONObject());
                        if (ctx != null)
                        {
                            if (ctx is JSONArray)
                                ((JSONArray)ctx)[((JSONArray)ctx).Count] = stack.Peek();
                            else if (ctx is JSONObject)
                                ((JSONObject)ctx)[tokenName] = stack.Peek();
                        }
                        tokenName = "";
                        token = "";
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (quoteMode)
                        {
                            token += aJSON[i];
                            break;
                        }
                        stack.Push(new JSONArray());
                        if (ctx != null)
                        {
                            if (ctx is JSONArray)
                                ((JSONArray)ctx)[((JSONArray)ctx).Count] = stack.Peek();
                            else if (ctx is JSONObject)
                                ((JSONObject)ctx)[tokenName] = stack.Peek();
                        }
                        tokenName = "";
                        token = "";
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (quoteMode)
                        {
                            token += aJSON[i];
                            break;
                        }
                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (token != "")
                        {
                            if (ctx is JSONArray)
                                ((JSONArray)ctx)[((JSONArray)ctx).Count] = ParseElement(token);
                            else if (ctx is JSONObject)
                                ((JSONObject)ctx)[tokenName] = ParseElement(token);
                        }
                        tokenName = "";
                        token = "";
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (quoteMode)
                        {
                            token += aJSON[i];
                            break;
                        }
                        tokenName = token;
                        token = "";
                        break;

                    case '"':
                        quoteMode ^= true;
                        break;

                    case ',':
                        if (quoteMode)
                        {
                            token += aJSON[i];
                            break;
                        }
                        if (token != "")
                        {
                            if (ctx is JSONArray)
                                ((JSONArray)ctx)[((JSONArray)ctx).Count] = ParseElement(token);
                            else if (ctx is JSONObject)
                                ((JSONObject)ctx)[tokenName] = ParseElement(token);
                        }
                        tokenName = "";
                        token = "";
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (quoteMode)
                            token += aJSON[i];
                        break;

                    case '\\':
                        ++i;
                        if (quoteMode)
                        {
                            char C = aJSON[i];
                            switch (C)
                            {
                                case 't': token += '\t'; break;
                                case 'r': token += '\r'; break;
                                case 'n': token += '\n'; break;
                                case 'b': token += '\b'; break;
                                case 'f': token += '\f'; break;
                                case 'u':
                                    {
                                        string s = aJSON.Substring(i + 1, 4);
                                        token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
                                        i += 4;
                                        break;
                                    }
                                default: token += C; break;
                            }
                        }
                        break;

                    default:
                        token += aJSON[i];
                        break;
                }
                ++i;
            }
            if (quoteMode)
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            return ctx ?? ParseElement(token);
        }

        private static JSONNode ParseElement(string token)
        {
            if (token == "")
                return null;
            if (token.ToLower() == "null")
                return new JSONNull();
            if (token.ToLower() == "true")
                return new JSONBool(true);
            if (token.ToLower() == "false")
                return new JSONBool(false);
            double n;
            if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
                return new JSONNumber(n);
            else
                return new JSONString(token);
        }
    }
} 