#
# Makefile para o br.ispell
#
#     make install            instala o hash e o conjugue
#     make conjugue           apenas corrige os paths do conjugue
#     make br.hash            apenas gera o hash
#     make palavras           apenas gera a lista de todas as palavras
#     make paradigmas         apenas mostra os paradigmas em ordem de uso
#

#
# Configure os seguintes paths:
#
#     HASHDIR - aqui será colocado o br.hash
#     BINDIR - aqui será colocado o conjugue
#     VDIR - aqui será colocado o banco de verbos
#     MANDIR - aqui será colocada a man page do conjugue
#
prefix=/usr/local
BINDIR=$(prefix)/bin
HASHDIR=/usr/lib/ispell
VDIR=$(prefix)/lib
MANDIR=$(prefix)/man
AWK=/usr/bin/gawk

#
# Daqui para baixo não mexa mais em nada
#

all: br.hash conjugue

#
# substitui os paths /usr/local/lib e /usr/bin/awk pelos que
# tiverem sido especificados acima.
#
conjugue: conjugue-stamp
conjugue-stamp:
	sed -e 's|/usr/bin/awk|$(AWK)|g' -e \
	's|/usr/local/lib/verbos|$(VDIR)/verbos|g' conjugue >conjugue.tmp
	chmod 755 conjugue.tmp
	mv -f conjugue.tmp conjugue
	sed -e 's|/usr/local/lib|$(VDIR)|g' conjugue.1 >conjugue.1.tmp
	chmod 644 conjugue.1.tmp
	mv -f conjugue.1.tmp conjugue.1
	touch conjugue-stamp

#
# Gera as conjugações de todos os verbos, com o resultado
# fatorado pelas flags do br.aff.
#
# O "pôr" é tratado separadamente por causa dos acentos, que
# impedem o uso do paradigma "propor". A conjugação completa
# do "pôr" consta de forma explícita do arquivo E.ispell.
#
# O segundo grep remove algumas linhas espúrias que o conjugue
# gera.
#
v.ispell: conjugue
	$(AWK) -f conjugue -v BANCO=verbos -v FORMATO=ci -v CMD=T|\
            grep -v "^pôr/"|grep -v "^/R" >v.tmp
	mv -f v.tmp v.ispell

#
# Gera o vocabulario total, flexionado com as flags do br.aff.
#
br.hash: v.ispell
	cat v.ispell E.ispell|sort|uniq >br.tmp
	mv -f br.tmp br.ispell
	buildhash br.ispell br.aff br.hash

#
# Instala o br.hash
#
install-hash: br.ispell
	install -d $(HASHDIR)
	install --mode=644 br.hash br.aff $(HASHDIR)

#
# Instala o conjugue
#
install-conjugue: conjugue
	install -d $(BINDIR)
	install --mode=755 conjugue $(BINDIR)/conjugue
	install -d $(VDIR)
	install --mode=644 verbos $(VDIR)/verbos
	install -d $(MANDIR)/man1
	install --mode=644 conjugue.1 $(MANDIR)/man1/conjugue.1

#
# instala tudo
#
install: install-hash install-conjugue

#
# Gera o vocabulário flexionado a partir do br.hash.
#
palavras: br.hash
	cat br.ispell | ispell -e -d ./br.hash | tr " " "\n" |sort|\
            uniq >palavras.tmp
	mv -f palavras.tmp palavras.ispell

#
# Lista todos os paradigmas em uso e o número de vezes que cada
# um é usado.
#
paradigmas: conjugue
	$(AWK) -f conjugue -v BANCO=verbos -v FORMATO=n -v CMD=T|\
            grep paradigma|sed 's/ deduzido//'|sort|uniq -c|sort

#
# Remove todos os arquivos construídos.
#
clean:
	rm -f v.tmp br.tmp palavras.tmp
	rm -f br.hash br.ispell.cnt br.ispell.stat
	rm -f v.ispell br.ispell
	rm -f palavras.ispell
	rm -f conjugue-stamp

