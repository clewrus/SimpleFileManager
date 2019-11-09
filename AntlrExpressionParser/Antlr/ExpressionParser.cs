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
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public partial class ExpressionParser : Parser {
	public const int
		T__0=1, T__1=2, INT=3, MUL=4, DIV=5, ADD=6, SUB=7, EQU=8, WS=9;
	public const int
		RULE_prog = 0, RULE_expr = 1;
	public static readonly string[] ruleNames = {
		"prog", "expr"
	};

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

	public override string SerializedAtn { get { return _serializedATN; } }


	    protected const int EOF = Eof;

	public ExpressionParser(ITokenStream input)
		: base(input)
	{
		_interp = new ParserATNSimulator(this,_ATN);
	}
	public partial class ProgContext : ParserRuleContext {
		public ExprContext[] expr() {
			return GetRuleContexts<ExprContext>();
		}
		public ExprContext expr(int i) {
			return GetRuleContext<ExprContext>(i);
		}
		public ProgContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_prog; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IExpressionVisitor<TResult> typedVisitor = visitor as IExpressionVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitProg(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ProgContext prog() {
		ProgContext _localctx = new ProgContext(_ctx, State);
		EnterRule(_localctx, 0, RULE_prog);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 5;
			_errHandler.Sync(this);
			_la = _input.La(1);
			do {
				{
				{
				State = 4; expr(0);
				}
				}
				State = 7;
				_errHandler.Sync(this);
				_la = _input.La(1);
			} while ( _la==T__0 || _la==INT );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ExprContext : ParserRuleContext {
		public ExprContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_expr; } }
	 
		public ExprContext() { }
		public virtual void CopyFrom(ExprContext context) {
			base.CopyFrom(context);
		}
	}
	public partial class MulDivContext : ExprContext {
		public ExprContext left;
		public IToken op;
		public ExprContext right;
		public ExprContext[] expr() {
			return GetRuleContexts<ExprContext>();
		}
		public ExprContext expr(int i) {
			return GetRuleContext<ExprContext>(i);
		}
		public MulDivContext(ExprContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IExpressionVisitor<TResult> typedVisitor = visitor as IExpressionVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitMulDiv(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class AddSubContext : ExprContext {
		public ExprContext left;
		public IToken op;
		public ExprContext right;
		public ExprContext[] expr() {
			return GetRuleContexts<ExprContext>();
		}
		public ExprContext expr(int i) {
			return GetRuleContext<ExprContext>(i);
		}
		public AddSubContext(ExprContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IExpressionVisitor<TResult> typedVisitor = visitor as IExpressionVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitAddSub(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class IntContext : ExprContext {
		public ITerminalNode INT() { return GetToken(ExpressionParser.INT, 0); }
		public IntContext(ExprContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IExpressionVisitor<TResult> typedVisitor = visitor as IExpressionVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitInt(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class ParensContext : ExprContext {
		public ExprContext expr() {
			return GetRuleContext<ExprContext>(0);
		}
		public ParensContext(ExprContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IExpressionVisitor<TResult> typedVisitor = visitor as IExpressionVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitParens(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ExprContext expr() {
		return expr(0);
	}

	private ExprContext expr(int _p) {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = State;
		ExprContext _localctx = new ExprContext(_ctx, _parentState);
		ExprContext _prevctx = _localctx;
		int _startState = 2;
		EnterRecursionRule(_localctx, 2, RULE_expr, _p);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 15;
			_errHandler.Sync(this);
			switch (_input.La(1)) {
			case INT:
				{
				_localctx = new IntContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;

				State = 10; Match(INT);
				}
				break;
			case T__0:
				{
				_localctx = new ParensContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				State = 11; Match(T__0);
				State = 12; expr(0);
				State = 13; Match(T__1);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.Lt(-1);
			State = 25;
			_errHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(_input,3,_ctx);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.InvalidAltNumber ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) TriggerExitRuleEvent();
					_prevctx = _localctx;
					{
					State = 23;
					_errHandler.Sync(this);
					switch ( Interpreter.AdaptivePredict(_input,2,_ctx) ) {
					case 1:
						{
						_localctx = new MulDivContext(new ExprContext(_parentctx, _parentState));
						((MulDivContext)_localctx).left = _prevctx;
						PushNewRecursionContext(_localctx, _startState, RULE_expr);
						State = 17;
						if (!(Precpred(_ctx, 4))) throw new FailedPredicateException(this, "Precpred(_ctx, 4)");
						State = 18;
						((MulDivContext)_localctx).op = _input.Lt(1);
						_la = _input.La(1);
						if ( !(_la==MUL || _la==DIV) ) {
							((MulDivContext)_localctx).op = _errHandler.RecoverInline(this);
						} else {
							if (_input.La(1) == TokenConstants.Eof) {
								matchedEOF = true;
							}

							_errHandler.ReportMatch(this);
							Consume();
						}
						State = 19; ((MulDivContext)_localctx).right = expr(5);
						}
						break;

					case 2:
						{
						_localctx = new AddSubContext(new ExprContext(_parentctx, _parentState));
						((AddSubContext)_localctx).left = _prevctx;
						PushNewRecursionContext(_localctx, _startState, RULE_expr);
						State = 20;
						if (!(Precpred(_ctx, 3))) throw new FailedPredicateException(this, "Precpred(_ctx, 3)");
						State = 21;
						((AddSubContext)_localctx).op = _input.Lt(1);
						_la = _input.La(1);
						if ( !(_la==ADD || _la==SUB) ) {
							((AddSubContext)_localctx).op = _errHandler.RecoverInline(this);
						} else {
							if (_input.La(1) == TokenConstants.Eof) {
								matchedEOF = true;
							}

							_errHandler.ReportMatch(this);
							Consume();
						}
						State = 22; ((AddSubContext)_localctx).right = expr(4);
						}
						break;
					}
					} 
				}
				State = 27;
				_errHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(_input,3,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			UnrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 1: return expr_sempred((ExprContext)_localctx, predIndex);
		}
		return true;
	}
	private bool expr_sempred(ExprContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0: return Precpred(_ctx, 4);

		case 1: return Precpred(_ctx, 3);
		}
		return true;
	}

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x3\v\x1F\x4\x2\t\x2"+
		"\x4\x3\t\x3\x3\x2\x6\x2\b\n\x2\r\x2\xE\x2\t\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x5\x3\x12\n\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\a\x3\x1A"+
		"\n\x3\f\x3\xE\x3\x1D\v\x3\x3\x3\x2\x2\x3\x4\x4\x2\x2\x4\x2\x2\x4\x3\x2"+
		"\x6\a\x3\x2\b\t \x2\a\x3\x2\x2\x2\x4\x11\x3\x2\x2\x2\x6\b\x5\x4\x3\x2"+
		"\a\x6\x3\x2\x2\x2\b\t\x3\x2\x2\x2\t\a\x3\x2\x2\x2\t\n\x3\x2\x2\x2\n\x3"+
		"\x3\x2\x2\x2\v\f\b\x3\x1\x2\f\x12\a\x5\x2\x2\r\xE\a\x3\x2\x2\xE\xF\x5"+
		"\x4\x3\x2\xF\x10\a\x4\x2\x2\x10\x12\x3\x2\x2\x2\x11\v\x3\x2\x2\x2\x11"+
		"\r\x3\x2\x2\x2\x12\x1B\x3\x2\x2\x2\x13\x14\f\x6\x2\x2\x14\x15\t\x2\x2"+
		"\x2\x15\x1A\x5\x4\x3\a\x16\x17\f\x5\x2\x2\x17\x18\t\x3\x2\x2\x18\x1A\x5"+
		"\x4\x3\x6\x19\x13\x3\x2\x2\x2\x19\x16\x3\x2\x2\x2\x1A\x1D\x3\x2\x2\x2"+
		"\x1B\x19\x3\x2\x2\x2\x1B\x1C\x3\x2\x2\x2\x1C\x5\x3\x2\x2\x2\x1D\x1B\x3"+
		"\x2\x2\x2\x6\t\x11\x19\x1B";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace AntlrExpressionParser.Antlr
