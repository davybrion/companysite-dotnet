I was looking for a way to define the mapping of an ID field so it would work on both Oracle and SQL Server... In case of Oracle, it would have to use a Sequence that I can define. On Sql Server, it should use an Identity column.  Luckily, NHibernate offers just that... the 'native' ID generator uses identity on sql server and a sequence on Oracle.

Unfortunately the 'native' generator is hardly documented in the Nhibernate reference documentation so it wasn't really clear to me which sequence would be used, or how I could define my own. All the documentation says is basically:

> For cross-platform development, the native strategy will choose from the identity, sequence and hilo strategies, dependent upon the capabilities of the underlying database.

So how can you define which sequence it should use in case of Oracle? Very simple actually... turns out you just define the sequence name in the same way you would as if you set it to use the 'sequence' generator.

So here's how you do it:

<script src="https://gist.github.com/3611435.js?file=s1.xml"></script>

On Oracle, it will use the sq_customer sequence whereas on SQL Server the sequence parameter will be ignored and it will use the Identity setting of the column that was defined as the ID (in this case, CustomerId's Identity settings)