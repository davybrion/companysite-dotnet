We're working on an application which has to use a legacy MySQL database.  So far, we haven't really had any problems with MySQL (apart from the dreadful legacy schema but that's another issue) but today, I started getting an exception with the following message:

<script src="https://gist.github.com/3684360.js?file=s1.txt"></script>

There is a certain complex view in our application which apparently hits this default limit set by MySQL once there is a certain amount of data present.  The view itself is a costly one... it really does need to do a whole lot of joins and rewriting it to use less joins would probably take a long time.

So I figured the better option in this case would be to set the SQL_BIG_SELECTS option to 1.  The only problem was: how? It's not a setting that you can pass through the connection string (or at least, I did not find a way to do so) and NHibernate is taking care of all of the database communication.

I remembered a trick I had used earlier, which is to extend NHibernate's DriverConnectionProvider.  It could then set the setting with the appropriate value whenever the connection is opened, like this:

<script src="https://gist.github.com/3684360.js?file=s2.cs"></script>

After that, you just set the DriverConnectionProvider to use in your hibernate.cfg.xml file like this:

<script src="https://gist.github.com/3684360.js?file=s3.xml"></script>

And all is well.