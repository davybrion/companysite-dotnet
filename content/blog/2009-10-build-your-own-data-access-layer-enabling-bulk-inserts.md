Note: This post is part of a series. Be sure to read the introduction <a href="/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

I know i wrapped up the series already, but i just had to add the ability to do bulk inserts to this data layer so i figured i'd might as well post about it.  Ayende already talked about how to enable the ability to batch inserts (or updates and deletes) <a href="http://ayende.com/Blog/archive/2006/09/13/OpeningUpQueryBatching.aspx">here</a> and <a href="http://ayende.com/Blog/archive/2006/09/14/ThereBeDragonsRhinoCommonsSqlCommandSet.aspx">here</a> so i'm going to skip that part.  I used the exact same trick and created a PublicSqlCommandSet class which wraps the hidden SqlCommandSet class.  Again, if you have no idea what i'm talking about in that last sentence then you need to read Ayende's 2 posts that i just linked to ;)

After that, adding the bulk insert feature to the DAL was as simple as creating this class:

<script src="https://gist.github.com/3685116.js?file=s1.cs"></script>

And then adding this to the Session class:

<script src="https://gist.github.com/3685116.js?file=s2.cs"></script>

Obviously, the method signature was also added to the ISession interface.  The batch-size and commandtimeout parameters are currently hardcoded but they should come from some kind of configuration file.

All in all, pretty easy stuff :)