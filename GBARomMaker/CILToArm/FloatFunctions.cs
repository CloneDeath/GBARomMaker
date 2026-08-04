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
	beq     gba_float_add_j8
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

gba_float_to_int:
	lsl     r2, r0, #1
	cmp     r2, #0x7f000000
	bcc     gba_float_to_int_j0
	mov     r3, #0x9e
	subs    r2, r3, r2, lsr #24
	bls     gba_float_to_int_j1
	lsl     r3, r0, #8
	orr     r3, r3, #0x80000000
	tst     r0, #0x80000000
	lsr     r0, r3, r2
	rsbne   r0, r0, #0
	bx      lr
gba_float_to_int_j0:
	mov     r0, #0
	bx      lr
gba_float_to_int_j1:
	cmn     r2, #0x61
	bne     gba_float_to_int_j2
	lsls    r2, r0, #9
	bne     gba_float_to_int_j3
gba_float_to_int_j2:
	ands    r0, r0, #0x80000000
	mvneq   r0, #0x80000000
	bx      lr
gba_float_to_int_j3:
	mov     r0, #0
	bx      lr

gba_uint_to_float:
	mov	r3, #0
	b	gba_int_to_float_j0

gba_int_to_float:
	ands	r3, r0, #0x80000000
	rsbmi	r0, r0, #0
gba_int_to_float_j0:
	movs	r4, r0
	bxeq	lr
	orr	r3, r3, #0x4b000000
	mov	r1, r0
	mov	r0, #0
	b	gba_long_to_float_j0

gba_ulong_to_float:
	orrs	r2, r0, r1
	bxeq	lr
	mov	r3, #0
	b	gba_long_to_float_j1

gba_long_to_float:
	orrs	r2, r0, r1
	bxeq	lr
	ands	r3, r1, #0x80000000
	bpl	gba_long_to_float_j1
	rsbs	r0, r0, #0
	rsc	r1, r1, #0
gba_long_to_float_j1:
	movs	r4, r1
	moveq	r4, r0
	moveq	r1, r0
	moveq	r0, #0
	orr	r3, r3, #0x5b000000
	subeq	r3, r3, #0x10000000
gba_long_to_float_j0:
	sub	r3, r3, #0x800000
	mov	r2, #23
	cmp	r4, #0x10000
	lsrcs	r4, r4, #16
	subcs	r2, r2, #16
	cmp	r4, #0x100
	lsrcs	r4, r4, #8
	subcs	r2, r2, #8
	cmp	r4, #16
	lsrcs	r4, r4, #4
	subcs	r2, r2, #4
	cmp	r4, #4
	subcs	r2, r2, #2
	subcc	r2, r2, r4, lsr #1
	subs	r2, r2, r4, lsr #3
	sub	r3, r3, r2, lsl #23
	blt	gba_long_to_float_j2
	add	r3, r3, r1, lsl r2
	lsl	r4, r0, r2
	rsb	r2, r2, #32
	cmp	r4, #0x80000000
	adc	r0, r3, r0, lsr r2
	biceq	r0, r0, #1
	bx	lr
gba_long_to_float_j2:
	add	r2, r2, #32
	lsl	r4, r1, r2
	rsb	r2, r2, #32
	orrs	r0, r0, r4, lsl #1
	adc	r0, r3, r1, lsr r2
	biceq	r0, r0, r4, lsr #31
	bx	lr

gba_float_mul:
	mov     r4, #0xff
	ands    r2, r4, r0, lsr #23
	andsne  r3, r4, r1, lsr #23
	teqne   r2, r4
	teqne   r3, r4
	beq     gba_float_mul_j0
gba_float_mul_j6:
	add     r2, r2, r3
	eor     r4, r0, r1
	lsls    r0, r0, #9
	lslsne  r1, r1, #9
	beq     gba_float_mul_j1
	mov     r3, #0x8000000
	orr     r0, r3, r0, lsr #5
	orr     r1, r3, r1, lsr #5
	umull   r3, r1, r0, r1
	and     r0, r4, #0x80000000
	cmp     r1, #0x800000
	lslcc   r1, r1, #1
	orrcc   r1, r1, r3, lsr #31
	lslcc   r3, r3, #1
	orr     r0, r0, r1
	sbc     r2, r2, #0x7f
	cmp     r2, #0xfd
	bhi     gba_float_mul_j2
	cmp     r3, #0x80000000
	adc     r0, r0, r2, lsl #23
	biceq   r0, r0, #1
	bx      lr
gba_float_mul_j1:
	teq     r0, #0
	and     r4, r4, #0x80000000
	lsleq   r1, r1, #9
	orr     r0, r4, r0, lsr #9
	orr     r0, r0, r1, lsr #9
	subs    r2, r2, #0x7f
	rsbsgt  r3, r2, #0xff
	orrgt   r0, r0, r2, lsl #23
	bxgt    lr
	orr     r0, r0, #0x800000
	mov     r3, #0
	subs    r2, r2, #1
gba_float_mul_j2:
	bgt     gba_float_mul_j3
	cmn     r2, #25
	andle   r0, r0, #0x80000000
	bxle    lr
	rsb     r2, r2, #0
	lsls    r1, r0, #1
	lsr     r1, r1, r2
	rsb     r2, r2, #32
	lsl     r4, r0, r2
	rrxs    r0, r1
	adc     r0, r0, #0
	orrs    r3, r3, r4, lsl #1
	biceq   r0, r0, r4, lsr #31
	bx      lr
gba_float_mul_j8:
	teq     r2, #0
	and     r4, r0, #0x80000000
gba_float_mul_j4:
	lsleq   r0, r0, #1
	tsteq   r0, #8388608    @ 0x800000
	subeq   r2, r2, #1
	beq     gba_float_mul_j4
	orr     r0, r0, r4
	teq     r3, #0
	and     r4, r1, #0x80000000
gba_float_mul_j5:
	lsleq   r1, r1, #1
	tsteq   r1, #8388608    @ 0x800000
	subeq   r3, r3, #1
	beq     gba_float_mul_j5
	orr     r1, r1, r4
	b       gba_float_mul_j6
gba_float_mul_j0:
	and     r3, r4, r1, lsr #23
	teq     r2, r4
	teqne   r3, r4
	beq     gba_float_mul_j7
	bics    r4, r0, #0x80000000
	bicsne  r4, r1, #0x80000000
	bne     gba_float_mul_j8
gba_float_mul_j12:
	eor     r0, r0, r1
	and     r0, r0, #0x80000000
	bx      lr
gba_float_mul_j7:
	teq     r0, #0
	teqne   r0, #0x80000000
	moveq   r0, r1
	teqne   r1, #0
	teqne   r1, #0x80000000
	beq     gba_float_mul_j9
	teq     r2, r4
	bne     gba_float_mul_j10
	lsls    r2, r0, #9
	bne     gba_float_mul_j9
gba_float_mul_j10:
	teq     r3, r4
	bne     gba_float_mul_j11
	lsls    r3, r1, #9
	movne   r0, r1
	bne     gba_float_mul_j9
gba_float_mul_j11:
	eor     r0, r0, r1
gba_float_mul_j3:
	and     r0, r0, #0x80000000
	orr     r0, r0, #0x7f000000
	orr     r0, r0, #0x800000
	bx      lr
gba_float_mul_j9:
	orr     r0, r0, #0x7f000000
	orr     r0, r0, #0xc00000
	bx      lr

gba_float_div:
	mov     r4, #0xff
	ands    r2, r4, r0, lsr #23
	andsne  r3, r4, r1, lsr #23
	teqne   r2, r4
	teqne   r3, r4
	beq     gba_float_div_j0
gba_float_div_j5:
	sub     r2, r2, r3
	eor     r4, r0, r1
	lsls    r1, r1, #9
	lsl     r0, r0, #9
	beq     gba_float_div_j2
	mov     r3, #0x10000000
	orr     r1, r3, r1, lsr #4
	orr     r3, r3, r0, lsr #4
	and     r0, r4, #0x80000000
	cmp     r3, r1
	lslcc   r3, r3, #1
	adc     r2, r2, #0x7d
	mov     r4, #0x800000
gba_float_div_j1:
	cmp     r3, r1
	subcs   r3, r3, r1
	orrcs   r0, r0, r4
	cmp     r3, r1, lsr #1
	subcs   r3, r3, r1, lsr #1
	orrcs   r0, r0, r4, lsr #1
	cmp     r3, r1, lsr #2
	subcs   r3, r3, r1, lsr #2
	orrcs   r0, r0, r4, lsr #2
	cmp     r3, r1, lsr #3
	subcs   r3, r3, r1, lsr #3
	orrcs   r0, r0, r4, lsr #3
	lsls    r3, r3, #4
	lsrsne  r4, r4, #4
	bne     gba_float_div_j1
	cmp     r2, #0xfd
	bhi     gba_float_mul_j2
	cmp     r3, r1
	adc     r0, r0, r2, lsl #23
	biceq   r0, r0, #1
	bx      lr
gba_float_div_j2:
	and     r4, r4, #0x80000000
	orr     r0, r4, r0, lsr #9
	adds    r2, r2, #0x7f
	rsbsgt  r3, r2, #0xff
	orrgt   r0, r0, r2, lsl #23
	bxgt    lr
	orr     r0, r0, #0x800000
	mov     r3, #0
	subs    r2, r2, #1
	b       gba_float_mul_j2
gba_float_div_j8:
	teq     r2, #0
	and     r4, r0, #0x80000000
gba_float_div_j3:
	lsleq   r0, r0, #1
	tsteq   r0, #0x800000
	subeq   r2, r2, #1
	beq     gba_float_div_j3
	orr     r0, r0, r4
	teq     r3, #0
	and     r4, r1, #0x80000000
gba_float_div_j4:
	lsleq   r1, r1, #1
	tsteq   r1, #0x800000
	subeq   r3, r3, #1
	beq     gba_float_div_j4
	orr     r1, r1, r4
	b       gba_float_div_j5
gba_float_div_j0:
	and     r3, r4, r1, lsr #23
	teq     r2, r4
	bne     gba_float_div_j6
	lsls    r2, r0, #9
	bne     gba_float_mul_j9
	teq     r3, r4
	bne     gba_float_mul_j11
	mov     r0, r1
	b       gba_float_mul_j9
gba_float_div_j6:
	teq     r3, r4
	bne     gba_float_div_j7
	lsls    r3, r1, #9
	beq     gba_float_mul_j12
	mov     r0, r1
	b       gba_float_mul_j9
gba_float_div_j7:
	bics    r4, r0, #0x80000000
	bicsne  r4, r1, #0x80000000
	bne     gba_float_div_j8
	bics    r2, r0, #0x80000000
	bne     gba_float_mul_j11
	bics    r3, r1, #0x80000000
	bne     gba_float_mul_j12
	b       gba_float_mul_j9

gba_sin:
	push sp!, { r9, r10, r11, lr }
	@ approx using: sin(x) ≈ x - x^3/6 + x^5/120
	
	@ r9, r10, r11 = x, x^3, x^5
	mov r9, r0
	mov r1, r9
	bl gba_float_mul
	mov r1, r9
	bl gba_float_mul
	mov r10, r0 @ x^3
	mov r1, r9
	bl gba_float_mul
	mov r1, r9
	bl gba_float_mul
	mov r11, r0 @ x^5
	
	@ r10 = x^3/6
	ldr r0, =6
	bl gba_int_to_float
	mov r1, r0
	mov r0, r10
	bl gba_float_div
	mov r10, r0

	@ r11 = x^5/120
	ldr r0, =120
	bl gba_int_to_float
	mov r1, r0
	mov r0, r11
	bl gba_float_div
	mov r11, r0

	@ subtract and add
	mov r0, r9
	mov r1, r10
	bl gba_float_subtract
	mov r1, r11
	bl gba_float_add

	pop sp!, { r9, r10, r11, lr }
	bx lr
	
	"
	.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	}

}
