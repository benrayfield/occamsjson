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
using System.Collections.Generic;

namespace Life3d.OccamsJson{
	
	public class TestJson{

		public static string n = Json.n;

		public static string anExampleOfJson = //TODO more test cases. jsoncpp has some meant to pass and fail
			"{"
			+n+"	\"rnrnabcrn\" : \"\\r\\n\\r\\nabc\\r\\n\"," // "rnrnabcrn" : "\r\n\r\nabc\r\n"
			+n+"	\"ab	\nc\" : 234,"
			+n+"	\"def\" : { \"ghi\" : \"JKL\", \"mn\" : 55589 },"
			+n+"	null : [false,true],"
			+n+"	false : [null,true],"
			+n+"	true : [false,false,null,false,true,3.14159],"
			+n+"	\"\" : \"value whose key is empty string\","
			+n+"	\"attribute\" : ["
			+n+"		\"ffjjj\","
			+n+"		\"opqr\","
			+n+"		\"stuv\","
			+n+"		5,"
			+n+"		{ \"aab\" : -7, false : 445 }"
			+n+"		],"
			+n+"	\"cce\": { \"22\" :"
			+n+"		{ \"22\" :"
			+n+"			{ null :  { \"ffggg\" : [ 5,-7.7] }"
			+n+"			}"
			+n+"		}"
			+n+"	}"
			+n+"}";

		static string esc(string s){
			return Json.escape(s);
		}

		public static void testParseThenReverse(string json){
			try{
				object parsed = Json.parse(json);
				Console.Out.WriteLine("parsed: "+parsed);
				IDictionary<object,object> parsedMap = (IDictionary<object,object>) parsed;
				object val = parsedMap["rnrnabcrn"];
				Console.Out.WriteLine("val of \"rnrnabcrn\" is ["+val+"]");
				parsedMap["rnrnabcrn_testAddValue"] = "\r\n\r\nabc\r\n";
				string reverse = Json.toJson(parsed);
				Console.Out.WriteLine("reverse: "+reverse);
			}catch(Exception e){
				Console.Out.WriteLine("Error "+e);
			}
		}


	}

}