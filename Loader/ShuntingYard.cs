using System;
using System.Collections.Generic;
using System.Linq;
using static mod_oni.Parentheses;

namespace mod_oni
{
	public class ShuntingYard
	{
		public string Expression;

		public ShuntingYard(string expression){
			this.Expression = expression;
		}

        private string[] Split(){
            var result = new List<string>();
            char[] separators = {' ', '~', '(', ')', '&', '|', '^'};
            string input = this.Expression;
    
            int startIndex = 0;
            int length = 0;
    
            for(int i = 0; i < input.Length; i++){
                if(separators.Contains(input[i])){
                    if(length > 0){
                        result.Add(input.Substring(startIndex, length));
                        length = 0;
                    }
    
                    if(input[i] != ' ') result.Add(input[i].ToString());
                }
                else{
                    if (length == 0) startIndex = i;
                    length++;
                }
            }
            
            if (length > 0) result.Add(input.Substring(startIndex, length));
            return result.ToArray();
        }

        private void ValidateTokens(List<Token> tokens){
            var parenthesesStack = new Stack<Parentheses>();
            Type prev = null;
            
            foreach(Token token in tokens){
                switch(token)
                {
                    case Port port: {
                        if(prev is Port) throw new Exception("Two ports in a row");
                        prev = token.GetType();
                        break;
                    }

                    case Operation operation: {
                        if(prev is Operation)
                        prev = token.GetType();
                        break;
                    }

                    case Parentheses parentheses: {
                        if(parentheses.GetType() == ParenthesesType.OPENING) parenthesesStack.Push(parentheses);
                        else{
                            if(parenthesesStack.Count > 0) parenthesesStack.Pop();
                            else throw new Exception("Missing opening parentheses");
                        }

                        break;
                    }
                }
            }
            
            // Parenteses de fechamento sobrando
            if(parenthesesStack.Count > 0) throw new Exception("Remaining opening parentheses");
        }
        
		public ReversePolishNotation ToReversePolishNotation(){
            var tokens = new List<Token>();
            
            foreach(string s in this.Split()) tokens.Add(s);

            //this.ValidateTokens(tokens);
            
            var output = new List<Item>();
            var operations = new Stack<IShuntingOp>();

            foreach(Token token in tokens)
            {
                switch(token)
                {
                    case Port port: {
                        output.Add(port);
                        break;
                    }

                    case Operation operation: {
                        while(operations.Count > 0 && operations.Peek().GetPrecedence() >= operation.GetPrecedence())
                            output.Add((Operation)operations.Pop());
                        
                        operations.Push(operation);
                        break;
                    }

                    case Parentheses parentheses: {
                        if(parentheses.GetType() == ParenthesesType.OPENING) operations.Push(parentheses);
                        else{
                            while(!operations.Peek().IsOpenParentheses())
                                output.Add((Operation)operations.Pop());
                            
                            operations.Pop();
                        }

                        break;
                    }
                }
            }

            foreach(IShuntingOp op in operations) output.Add((Operation)op);
            return new ReversePolishNotation(output);
        }
	}
}