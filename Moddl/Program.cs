using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl {
	class Program {
		static void Main(string[] args) {
			var moddl = 
@"
a	o5L4
a	g+d+ef+ g2f+e e1 d+2...r16
a	c2e2 d+2^8.r16<b> e2g2 f+2...r16
a	e2g2 f+2^8.r16d+4 a2>c2< bag+f+
b	o4L8
b	b2g4ab> c4<b4a4g4 f+2ef+ga b4f+ga4d+4
b	e4ede4g4 f+4f+ef+2 >c4<cdef+gab4f+4d+4<b4>
b	g2e4g4 <b4b>c+d+2 e4f+4g4e4 d+4c+4<b4a4
c	o3L2
c	ec <a>d< b>f+ b<b>>
c	L8 c4<cdef+ga brf+rd+r<br> erede4c4 f+rf+ef+2
c	cdef+g4f+e d+ef+ga4b4> L2 c<a f+<b
abc y`duty`,0.25 [0
a	o4L4
a	b>ef+<b> a2g+f+ ed+8e8f+ee2d+2 c+f+g+c+ b2ag+ f+f8g+8f+c+ g+2f+2<
a	b>ef+<b> a2g+f+ ed+8e8f+ee2d+2 c+f+g+c+ b2ag+ f+f8g+8f+c+ g+2f+f+16g+16a16b16>
a	c+2^8r8c+< b2^8r8g+ aa8g+8f+f f+g+ab>
a	d2^8r8d c+2^8r8<a bb8>c8<bb8>c8< bag+f+<
b	o3L4
b	b2.>c+8d+8ec+d+f+ c+d+8e8f+g+8a+8 bf+d+<b> f+2ff+8g+8 af+fb a2a+2 b2a2<
b	b2.>c+8d+8ec+d+f+ c+d+8e8f+g+8a+8 bf+d+<b> f+2ff+8g+8 af+fb a2a+2 b2a2
b	L8 e4ag+a4e4 e4bag+f+eg+> d<af+4g+c+d+f f+4<b>c+def+4
b	g2^gab a2^agf+ e4edcde4 d+2e4f+4<
b	L4
c	o3L2
c	ed+ c+<b aa+ b1> ag+ f+f f+e d+<b>
c	ed+ c+c c+<a+ b1> ag+ f+f f+e d+<b>
c	L8 ar<ab>c+d+ef+ g+rg+f+g+rer f+4d4c+4bg+ f+4e4d4c+4<
c	br>babrgr f+r>c+r<ar<ab> cr>cr<ef+g4 f+4d+4<b4a4>
c	L2
abc ]
";
			new Player().Play(moddl);
		}
	}
}
