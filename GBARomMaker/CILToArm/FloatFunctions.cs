using System;

namespace GBARomMaker.CILToArm;

// https://fossies.org/linux/gcc/libgcc/config/arm/ieee754-sf.S
public static class FloatFunctions {
	public static string[] GetLines() {
		return @"
gba_float_subtract:
	eor     r1, r1, #0x80000000 

gba_float_add:
	lsls    r2, r0, #1
	lslsne  r3, r1, #1
	teqne   r2, r3
	mvnsne  r4, r2, asr #24
	mvnsne  r4, r3, asr #24
	beq     gba_float_add_j1
	lsr     r2, r2, #24
	rsbs    r3, r2, r3, lsr #24
	addgt   r2, r2, r3
	eorgt   r1, r0, r1
	eorgt   r0, r1, r0
	eorgt   r1, r0, r1
	rsblt   r3, r3, #0
	cmp     r3, #25
	bxhi    lr
	tst     r0, #0x80000000
	orr     r0, r0, #0x800000
	bic     r0, r0, #0xff000000
	rsbne   r0, r0, #0
	tst     r1, #0x80000000
	orr     r1, r1, #0x800000
	bic     r1, r1, #0xff000000
	rsbne   r1, r1, #0
	teq     r2, r3
	beq     gba_float_add_j2
gba_float_add_j7:
	sub     r2, r2, #1
	adds    r0, r0, r1, asr r3
	rsb     r3, r3, #32
	lsl     r1, r1, r3
	and     r3, r0, #0x80000000
	bpl     gba_float_add_j3
	rsbs    r1, r1, #0
	rsc     r0, r0, #0
gba_float_add_j3:
	cmp     r0, #0x800000
	bcc     gba_float_add_j4
	cmp     r0, #0x1000000
	bcc     gba_float_add_j5
	lsrs    r0, r0, #1
	rrx     r1, r1
	add     r2, r2, #1
	cmp     r2, #0xfe
	bcs     gba_float_add_j6
gba_float_add_j5:
	cmp     r1, #0x80000000
	adc     r0, r0, r2, lsl #23
	biceq   r0, r0, #1
	orr     r0, r0, r3
	bx      lr
gba_float_add_j4:
	lsls    r1, r1, #1
	adc     r0, r0, r0
	subs    r2, r2, #1
	cmpcs   r0, #0x800000
	bcs     gba_float_add_j5
	lsrs    r4, r0, #12
	lsleq   r0, r0, #12
	subeq   r2, r2, #12
	tst     r0, #0xff0000
	lsleq   r0, r0, #8
	subeq   r2, r2, #8
	tst     r0, #0xf00000
	lsleq   r0, r0, #4
	subeq   r2, r2, #4
	tst     r0, #0xc00000
	lsleq   r0, r0, #2
	subeq   r2, r2, #2
	cmp     r0, #0x800000
	lslcc   r0, r0, #1
	sbcs    r2, r2, #0
	addge   r0, r0, r2, lsl #23
	rsblt   r2, r2, #0
	orrge   r0, r0, r3
	orrlt   r0, r3, r0, lsr r2
	bx      lr
gba_float_add_j2:
	teq     r2, #0
	eor     r1, r1, #0x800000
	eoreq   r0, r0, #0x800000
	addeq   r2, r2, #1
	subne   r3, r3, #1
	b       gba_float_add_j7
gba_float_add_j1:
	lsl     r3, r1, #1
	mvns    r4, r2, asr #24
	mvnsne  r4, r3, asr #24
	beq     gba_fload_add_j8
	teq     r2, r3
	beq     gba_float_add_j9
	teq     r2, #0
	moveq   r0, r1
	bx      lr
gba_float_add_j9:
	teq     r0, r1
	movne   r0, #0
	bxne    lr
	tst     r2, #0xff000000
	bne     gba_float_add_j10
	lsls    r0, r0, #1
	orrcs   r0, r0, #0x80000000
	bx      lr
gba_float_add_j10:
	adds    r2, r2, #0x2000000
	addcc   r0, r0, #0x800000
	bxcc    lr
	and     r3, r0, #0x80000000
gba_float_add_j6:
	orr     r0, r3, #0x7f000000
	orr     r0, r0, #0x800000
	bx      lr
gba_float_add_j8:
	mvns    r2, r2, asr #24
	movne   r0, r1
	mvnseq  r3, r3, asr #24
	movne   r1, r0
	lsls    r2, r0, #9
	lslseq  r3, r1, #9
	teqeq   r0, r1
	orrne   r0, r0, #0x400000
	bx      lr
	"
	.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	}

}
