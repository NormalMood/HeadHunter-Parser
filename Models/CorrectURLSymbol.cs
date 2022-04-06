using System;
using System.Collections.Generic;

namespace HeadHunter_Parser.Models
{
    public struct CorrectURLSymbol
    {

        public static IReadOnlyList<CorrectURLSymbol> CorrectURLSymbols = new[]
        { 
            new CorrectURLSymbol('+'),
            new CorrectURLSymbol('#'),
            new CorrectURLSymbol('&'),
            new CorrectURLSymbol('$'),
            new CorrectURLSymbol('@'),
            new CorrectURLSymbol('!'),
            new CorrectURLSymbol('%'),
            new CorrectURLSymbol('^'),
            new CorrectURLSymbol('('),
            new CorrectURLSymbol(')'),
            new CorrectURLSymbol('`'),
            new CorrectURLSymbol('\''),
            new CorrectURLSymbol('?'),
            new CorrectURLSymbol('='),
            new CorrectURLSymbol(':'),
            new CorrectURLSymbol('|'),
            new CorrectURLSymbol('\\'),
            new CorrectURLSymbol('/'),
            new CorrectURLSymbol(','),
            new CorrectURLSymbol(';'),
            new CorrectURLSymbol(' ', "+")
        };

        public char InitialSymbol { get; private set; }

        public string CorrectedSymbol { get; private set; }

        private CorrectURLSymbol(char initialSymbol, string correctedSymbol = null)
        {
            InitialSymbol = initialSymbol;
            //+ # & $ @ ! % ^ () ` ' ? = : | / \ , ;   <- replace to %hex and spaces to +
            if (correctedSymbol == null)
                CorrectedSymbol = $"%{Convert.ToString(InitialSymbol, 16).ToUpper()}";
            else
                CorrectedSymbol = correctedSymbol;
        }

    }
}
