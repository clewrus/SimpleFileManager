﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Алексей\VSProjects\SimpleFM\AntlrExpressionParser\Antlr\Expression.g4 by ANTLR 4.6.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace AntlrExpressionParser.Antlr {
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public partial class ExpressionLexer : Lexer {
	public const int
		T__0=1, T__1=2, INT=3, MUL=4, DIV=5, ADD=6, SUB=7, EQU=8, WS=9;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "INT", "MUL", "DIV", "ADD", "SUB", "EQU", "WS"
	};


	    protected const int EOF = Eof;
	    protected const int HIDDEN = Hidden;


	public ExpressionLexer(ICharStream input)
		: base(input)
	{
		_interp = new LexerATNSimulator(this,_ATN);
	}

	private static readonly string[] _LiteralNames = {
		null, "'('", "')'", null, "'*'", "'/'", "'+'", "'-'", "'='"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, "INT", "MUL", "DIV", "ADD", "SUB", "EQU", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[System.Obsolete("Use Vocabulary instead.")]
	public static readonly string[] tokenNames = GenerateTokenNames(DefaultVocabulary, _SymbolicNames.Length);

	private static string[] GenerateTokenNames(IVocabulary vocabulary, int length) {
		string[] tokenNames = new string[length];
		for (int i = 0; i < tokenNames.Length; i++) {
			tokenNames[i] = vocabulary.GetLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = vocabulary.GetSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}

		return tokenNames;
	}

	[System.Obsolete("Use IRecognizer.Vocabulary instead.")]
	public override string[] TokenNames
	{
		get
		{
			return tokenNames;
		}
	}

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Expression.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x2\v,\b\x1\x4\x2\t"+
		"\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b\x4\t"+
		"\t\t\x4\n\t\n\x3\x2\x3\x2\x3\x3\x3\x3\x3\x4\x6\x4\x1B\n\x4\r\x4\xE\x4"+
		"\x1C\x3\x5\x3\x5\x3\x6\x3\x6\x3\a\x3\a\x3\b\x3\b\x3\t\x3\t\x3\n\x3\n\x3"+
		"\n\x3\n\x2\x2\x2\v\x3\x2\x3\x5\x2\x4\a\x2\x5\t\x2\x6\v\x2\a\r\x2\b\xF"+
		"\x2\t\x11\x2\n\x13\x2\v\x3\x2\x4\x3\x2\x32;\x5\x2\f\f\xF\xF\"\",\x2\x3"+
		"\x3\x2\x2\x2\x2\x5\x3\x2\x2\x2\x2\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v"+
		"\x3\x2\x2\x2\x2\r\x3\x2\x2\x2\x2\xF\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x2"+
		"\x13\x3\x2\x2\x2\x3\x15\x3\x2\x2\x2\x5\x17\x3\x2\x2\x2\a\x1A\x3\x2\x2"+
		"\x2\t\x1E\x3\x2\x2\x2\v \x3\x2\x2\x2\r\"\x3\x2\x2\x2\xF$\x3\x2\x2\x2\x11"+
		"&\x3\x2\x2\x2\x13(\x3\x2\x2\x2\x15\x16\a*\x2\x2\x16\x4\x3\x2\x2\x2\x17"+
		"\x18\a+\x2\x2\x18\x6\x3\x2\x2\x2\x19\x1B\t\x2\x2\x2\x1A\x19\x3\x2\x2\x2"+
		"\x1B\x1C\x3\x2\x2\x2\x1C\x1A\x3\x2\x2\x2\x1C\x1D\x3\x2\x2\x2\x1D\b\x3"+
		"\x2\x2\x2\x1E\x1F\a,\x2\x2\x1F\n\x3\x2\x2\x2 !\a\x31\x2\x2!\f\x3\x2\x2"+
		"\x2\"#\a-\x2\x2#\xE\x3\x2\x2\x2$%\a/\x2\x2%\x10\x3\x2\x2\x2&\'\a?\x2\x2"+
		"\'\x12\x3\x2\x2\x2()\t\x3\x2\x2)*\x3\x2\x2\x2*+\b\n\x2\x2+\x14\x3\x2\x2"+
		"\x2\x4\x2\x1C\x3\x2\x3\x2";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace AntlrExpressionParser.Antlr
