
uStatKings.SqlServerAdoNet.QueryHelper.ParameterizeInClauseQuery<T>(string, System.Collections.Generic.IEnumerable<T>)L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csK h(	queryinClauseData"0*ü
0ë
é
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csM M(0
%0"!string.IsNullOrWhiteSpace(string)*
"
string*	

query*
1
2*®
1pn
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csO O(R
%1"System.ArgumentException´
®
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csO O(+
%2":System.ArgumentException.ArgumentException(string, string)*

%1*
""*
""*
3*≈
2ë
é
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csQ* Q(L
%3"__id*2*0
System.StringComparison"
OrdinalIgnoreCase£
†
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csQ Q(M
%4"/string.IndexOf(string, System.StringComparison)*	

query*
""*

%3*
4
5*®
4pn
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csS S(f
%5"System.ArgumentException´
®
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csS S(+
%6":System.ArgumentException.ArgumentException(string, string)*

%5*
""*
""*
3*Ø
5°
û
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csU U($
%7""object.operator ==(object, object)*
"
object*

inClauseData*
""*
6
7*È
7€
ÿ
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csU) U(;
%8"TSystem.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>)*"
System.Linq.Enumerable*

inClauseData*
6
8*©
6pn
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csW W(h
%9"System.ArgumentException¨
©
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csW W(+
%10":System.ArgumentException.ArgumentException(string, string)*

%9*
""*
""*
3*Ô
8ï
í
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.cs[ [(h
%11"ÑSystem.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, TResult>)*"
System.Linq.Enumerable*

inClauseData*
""◊
‘
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.cs[ [(r
%12"XSystem.Linq.Enumerable.ToArray<TSource>(System.Collections.Generic.IEnumerable<TSource>)*"
System.Linq.Enumerable*

%11m
k
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.cs[ [(r

paramNames"__id*

%12¢
ü
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.cs^0 ^(L
%13"$string.Join(string, params string[])*
"
string*
""*


paramNamesó
î
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.cs^ ^(M
%14"string.Format(string, object)*
"
string*	

query*

%13k
i
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.cs^ ^(M

outQuery"__id*

%14{y
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csa a(3
%15""System.Collections.Generic.List<T>å
â
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csa a(1
%16")System.Collections.Generic.List<T>.List()*

%15g
e
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csa a(3
prms"__id*

%15c
a
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csb b(
i"__id*
""*
9*ê
9Ä
~
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csb  b(1
%17"System.Array.Length.get*


paramNames*
10
11*Ñ
10s
q
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csd* d(7
%18"
__arrayGet*


paramNamesÓ
Î
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csd9 d(R
%19"_System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*"
System.Linq.Enumerable*

inClauseData*

iƒ
¡
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csd9 d(]
%20"6StatKings.SqlServerAdoNet.QueryHelper.ToDBNull(object)*)"'
%StatKings.SqlServerAdoNet.QueryHelper*

%19{y
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csd d(^
%21""System.Data.SqlClient.SqlParameter¥
±
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csd d()
%22"?System.Data.SqlClient.SqlParameter.SqlParameter(string, object)*

%21*

%18*

%20ñ
ì
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csd d(_
%23")System.Collections.Generic.List<T>.Add(T)*

prms*

%21*
12*	
12*
9*‚
11ÜÉ
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csg g(S
%24",StatKings.SqlServerAdoNet.InClauseProperties§
°
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csg g()
%25"AStatKings.SqlServerAdoNet.InClauseProperties.InClauseProperties()*

%24ß
§
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csg. g(>
%26"6StatKings.SqlServerAdoNet.InClauseProperties.Query.set*

%24*


outQuery®
•
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csg@ g(Q
%27";StatKings.SqlServerAdoNet.InClauseProperties.Parameters.set*

%24*

prms"W
L
BC:\Projects\SqlServerAdoNet\SqlServerAdoNet\Helpers\QueryHelper.csg g(T

%24*
3"
""