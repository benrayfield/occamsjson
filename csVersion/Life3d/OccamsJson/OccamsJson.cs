/* OccamsJson: MIT license text copied from https://en.wikipedia.org/wiki/MIT_License 2016-02-05:

Copyright 2016 "Life3D, LLC". Author "Ben F. Rayfield".

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Text;
using System.Collections.Generic;

namespace Life3d.OccamsJson{

	/** Functions parse(string) and toJson(object) create and use any acyclicNet of these types:
	Dictionary<object,object>, List<object>, string, double, true, false, or Life3dJson.nullObject.
	*/
	public class Json{

		/** TODO instead of unsafe, use ref of int, like in https://msdn.microsoft.com/en-us/library/14akc2c7.aspx */
		public static object parse(string json){
			string[] tokens = tokenize(json);
			return parseTokens(tokens);
		}

		public static string toJson(object mapOrList){
			StringBuilder sb = new StringBuilder();
			toJson(sb, mapOrList, 0);
			return sb.ToString();
		}
		
		public static readonly string n = "\r\n";

		public static readonly object nullObject = new NullObject();
		class NullObject{};

		public static object parseNumber(string jsonNumber){
			if (jsonNumber.Contains(".")) return Double.Parse(jsonNumber);
			return long.Parse(jsonNumber);
		}

		public static object parseTokens(string[] tokens){
			int whichTokenInt = 0;
			return parseTokens(tokens, ref whichTokenInt);
		}

		/** After each range of tokens is consumed, *whichToken is the index right after it.
		String literal, number, booleans, and null are single token. Brackets recurse.
		Unsafe part is int pointer. Its target value is added to during recursive parsing.
		TODO change that to int[] size 1? It would remove the need for unsafe keyword.
		*/
		public static object parseTokens(string[] tokens, ref int whichToken){
			string firstToken = tokens[whichToken];
			whichToken++;
			char firstChar = firstToken[0];
			if(firstChar == '{'){
				
				Dictionary<object,object> list = new Dictionary<object,object>();
				while(whichToken < tokens.Length){
					string token = tokens[whichToken];
					whichToken++;
					if(token.Equals("}")){
						return list;
					}else{
						whichToken--;  //parse starting with what could have been ]
						object key = parseTokens(tokens, ref whichToken);
						string nextToken = tokens[whichToken];
						whichToken++;
						if(!nextToken.Equals(":")) throw new Exception("Expected colon after key but got "+nextToken);
						object value = parseTokens(tokens, ref whichToken);
						list.Add(key, value);
						if(whichToken == tokens.Length) throw new Exception("Unclosed dictionary/map at end");
						nextToken = tokens[whichToken];
						whichToken++;
						if(nextToken.Equals("}")){
							return list;
						}else{ //then it must be comma
							if(!nextToken.Equals(",")) throw new Exception(
								"Expected comma at token index "+whichToken);
						}
					}
				}
				throw new Exception("Unclosed dictionary/map at end");
			}else if(firstToken[0] == '['){
				List<object> list = new List<object>();
				while(whichToken < tokens.Length){
					string token = tokens[whichToken];
					whichToken++;
					if(token.Equals("]")){
						return list;
					}else{
						whichToken--;  //parse starting with what could have been ]
						object o = parseTokens(tokens, ref whichToken);
						list.Add(o);
						if(whichToken == tokens.Length) throw new Exception("Unclosed list at end");
						string nextToken = tokens[whichToken];
						whichToken++;
						if(nextToken.Equals("]")){
							return list;
						}else{ //then it must be comma
							if(!nextToken.Equals(",")) throw new Exception(
								"Expected comma at token index "+whichToken+" firstToken="+firstToken+" nextToken="+nextToken);
						}
					}
				}
				throw new Exception("Unclosed list at end. firstToken="+firstToken);
			}else if(firstChar == '"'){
				return unescape(firstToken);
			}else if(firstChar == '-' || firstChar == '.' || Char.IsDigit(firstChar)){
				return parseNumber(firstToken);
			}else if(firstChar == 't' && "true".Equals(firstToken)){
				return true;
			}else if(firstChar == 'f' && "false".Equals(firstToken)){
				return false;
			}else if(firstChar == 'n' && "null".Equals(firstToken)){
				return nullObject;
			}
			throw new Exception("First token (in a recursion, whichTokenInt="+whichToken
				+") must be { or [ or quote or - or digit or in true, false, or null,"
				+" but its length is "+firstToken.Length+" and string is "+firstToken);
		}

		public static string[] tokenize(string json){
			List<string> tokens = new List<string>();
			string[] bigTokens = Json.tokenizeStringLiterals(json);
			for(int i=0; i<bigTokens.Length; i++){
				if(bigTokens[i][0] == '"'){
					tokens.Add(bigTokens[i]);
				}else{
					string[] smallTokens = Json.tokenizeBetweenStringLiterals(bigTokens[i]);
					for(int s=0; s<smallTokens.Length; s++){
						tokens.Add(smallTokens[s]);
					}
				}
			}
			return tokens.ToArray();
		}

		public static string escape(string s){
			StringBuilder sb = new StringBuilder();
			sb.Append('"');
			for(int i=0; i<s.Length; i++){
				char c = s[i];
				switch(c){
					case '\t': sb.Append("\\t"); break;
					case '\r': sb.Append("\\r"); break;
					case '\n': sb.Append("\\n"); break;
					case '\\': sb.Append("\\\\"); break;
					case '\"': sb.Append("\\\""); break;
					default: sb.Append(c); break;
				}
			}
			sb.Append('"');
			return sb.ToString();
		}

		public static object unescape(string quote){
			if(quote[0] != '"') throw new Exception("Does not start with quote: "+quote);
			bool backslash = false;
			StringBuilder sb = new StringBuilder();
			for(int i=1; i<quote.Length; i++){
				char c = quote[i];
				if(backslash){
					switch(c){
					case 't': sb.Append('\t'); break;
					case 'r': sb.Append('\r'); break;
					case 'n': sb.Append('\n'); break;
					case '\\': sb.Append('\\'); break;
					case '"': sb.Append('"'); break;
					//case '\r': case '\n':
					//	throw new Exception("Unclosed string literal before line end. quote["+quote+"]");
					//break;
					default:
						//TODO do we want error if unrecognized escape code?
						//sb.Append('\\').Append(c);
						sb.Append(c);
					break;
					}
					backslash = false;
				}else{ // !backslash
					switch(c){
					case '"':
						if(i != quote.Length-1) throw new Exception("String literal ended early at char index "+i
							+" of "+quote.Length+" in stringLiteral["+quote+"]");
						return sb.ToString();
					case '\\':
						backslash = true;
					break;
					default:
						sb.Append(c);
					break;
					}
				}
			}
			throw new Exception("String literal did not end ["+quote+"]");
		}

		/** Returns string literals (including quotes and escape codes) and whats between them.
		Whats between them has multiple tokens per token returned here, to be further parsed.
		*/
		public static string[] tokenizeStringLiterals(string json){
			char[] chars = json.ToCharArray();
			bool inStrLiteral = false;
			bool backslash = false;
			List<string> tokens = new List<string>();
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < json.Length; i++){
				char c = chars[i];
				//Start.log("Parsing char: "+c);
				if(inStrLiteral){
					if(backslash){
						switch(c){
						case 't': sb.Append("\\t"); break;
						case 'r': sb.Append("\\r"); break;
						case 'n': sb.Append("\\n"); break;
						case '\\': sb.Append("\\\\"); break;
						case '"': sb.Append("\\\""); break;
						//case '\r': case '\n':
						//	if(inStrLiteral) throw new Exception("Unclosed string literal before line end");
						//break;
						default:
							//TODO do we want error if unrecognized escape code?
							//sb.Append('\\').Append(c);
							sb.Append(c);
						break;
						}
						backslash = false;
					}else{ //inStrLiteral && !backslash
						switch(c){
						case '"':
							sb.Append('"');
							string token = sb.ToString();
							//Start.log("Parsed token: "+token);
							//Start.log(n+"=== Parsed token inStrLiteral: "+token+" ==="+n);
							tokens.Add(token);
							sb.Clear();
							//inStrLiteral = backslash = inOtherLiteral = false;
							inStrLiteral = backslash = false;
							//Also at end check for unclosed string literal
						break;
						case '\\':
							backslash = true;
						break;
						default:
							sb.Append(c);
						break;
						}
					}
				}else{ // !inStringLiteral
					if(c=='"'){
						if(sb.Length != 0){
							string token = sb.ToString();
							//Start.log(n+"=== Parsed token !inStrLiteral: "+token+" ==="+n);
							tokens.Add(token);
							sb.Clear();
							//inStrLiteral = backslash = inOtherLiteral = false;
							inStrLiteral = backslash = false;
						}
						if(c == '"'){
							sb.Append('"');
							inStrLiteral = true;
						}
					}else{
						sb.Append(c);
					}
				}
			}
			if(inStrLiteral) throw new Exception("Unclosed string literal");
			//Get last nonStringLiteral token which ends here.
			if(sb.Length != 0){
				string token = sb.ToString();
				//Start.log("Parsed token: "+token);
				//Start.log(n+"=== Parsed token at end: "+token+" ==="+n);
				tokens.Add(token);
			}
			return tokens.ToArray();
		}



		/** Tokenize 1 of the strings returned by tokenizeStringLiterals, those that arent string literals.
		Each will become 1 or more tokens.
		*/
		public static string[] tokenizeBetweenStringLiterals(string json){
			//Start.log("START tokenizeBetweenStringLiterals: "+json);
			char[] chars = json.ToCharArray();
			List<string> tokens = new List<string>();
			StringBuilder sb = new StringBuilder();
			bool prevOneCharToken = false;
			for(int i = 0; i < json.Length; i++){
				char c = chars[i];
				bool whitespace = Char.IsWhiteSpace(c);
				//Start.log("Parsing char: "+c);
				bool oneCharTokenNow = c==',' || c==':' || c == '{' || c == '}' || c == '[' || c == ']';
				if(prevOneCharToken || whitespace || oneCharTokenNow){
					if(sb.Length != 0){
						string token = sb.ToString();
						//Start.log(n+"=== Parsed token: "+token+" ==="+n);
						tokens.Add(token);
						sb.Clear();
					}
				}
				if(!whitespace) sb.Append(c);
				prevOneCharToken = oneCharTokenNow; //for if next token starts after a comma etc
			}
			//Get last token which ends here
			if(sb.Length != 0){
				string token = sb.ToString();
				//Start.log(n+"=== Parsed token at end: "+token+" ==="+n);
				tokens.Add(token);
			}
			//Start.log("END tokenizeBetweenStringLiterals: "+json);
			return tokens.ToArray();
		}

		public static readonly Type listType = new List<object>().GetType();

		public static readonly Type mapType = new Dictionary<object,object>().GetType();

		public static readonly Type stringType = "".GetType();

		public static readonly Type scalarType = 0.0.GetType();

		public static readonly Type longType = 0L.GetType();

		public static void toJson(StringBuilder sb, object acyclicNet, int recurse){
			appendLineThenTabs(sb, recurse);
			if(acyclicNet == nullObject || acyclicNet == null){
				sb.Append("null");
				return;
			}
			Type t = acyclicNet.GetType();
			if(Object.ReferenceEquals(t, listType)){
				sb.Append('[');
				List<object> list = (List<object>) acyclicNet;
				for(int i=0; i<list.Count; i++){
					object o = list[i];
					toJson(sb, o, recurse+1);
					if(i != list.Count-1) sb.Append(',');
				}
				appendLineThenTabs(sb, recurse);
				sb.Append(']');
			}else if(Object.ReferenceEquals(t, mapType)){
				sb.Append('{');
				Dictionary<object,object> map = (Dictionary<object,object>) acyclicNet;
				int i = 0;
				foreach(object key in map.Keys){
					object value = map[key];
					toJson(sb, key, recurse+1);
					sb.Append(" :");
					toJson(sb, value, recurse+1);
					if(i != map.Count-1) sb.Append(',');
					i++;
				}
				appendLineThenTabs(sb, recurse);
				sb.Append('}');
			}else if(Object.ReferenceEquals(t, stringType)){
				sb.Append(escape((string)acyclicNet));
			}else if (Object.ReferenceEquals(t, scalarType)){
				//TODO leave it as scalar?
				double d = (double)acyclicNet;
				string s = ""+d;
				if(!s.Contains(".")) s += ".0";
				sb.Append(s);
			}else if(Object.ReferenceEquals(t, longType)){
				sb.Append((long)acyclicNet);
			}else if(acyclicNet.Equals(true)){
				sb.Append("true");
			}else if(acyclicNet.Equals(false)){
				sb.Append("false");
			}else throw new Exception("Object type not recognized: "+acyclicNet);
		}

		public static void appendLineThenTabs(StringBuilder sb, int tabs){
			sb.Append(n);
			for(int i=0; i<tabs; i++) sb.Append('\t');
		}

	}

}
