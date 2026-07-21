namespace GBARomMaker.ARM.Common;

// https://problemkaputt.de/gbatek-arm-cpu-flags-condition-field-cond.htm
public enum Condition {
	EQ = 0,
	Equal = 0,

	NE = 1,
	NotEqual = 1,
	
	HS = 2,
	CS = 2,
	UnsignedHigherOrSame = 2,
	
	LO = 3,
	CC = 3,
	UnsignedLower = 3,

	MI = 4,
	Minus = 4,
	SignedNegative = 4,
	
	PL = 5,
	Plus = 5,
	SignedPositiveOrZero = 5,
	
	VS = 6,
	VSet = 6,
	SignedOverflow = 6,
	
	VC = 7,
	VCleared = 7,
	SignedNoOverflow = 7,
	
	HI = 8,
	UnsignedHigher = 8,
	
	LS = 9,
	UnsignedLowerOrSame = 9,
	
	GE = 10,
	SignedGreaterOrEqual = 10,
	
	LT = 11,
	SignedLessThan = 11,
	
	GT = 12,
	SignedGreaterThan = 12,
	
	LE = 13,
	SignedLessOrEqual = 13,
	
	AL = 14,
	Always = 14,
	
	NV = 15,
	Never = 15
}
