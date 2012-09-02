We're working on an application which has to use a legacy MySQL database.  So far, we haven't really had any problems with MySQL (apart from the dreadful legacy schema but that's another issue) but today, i started getting an exception with the following message:

The SELECT would examine more than MAX_JOIN_SIZE rows; check your WHERE and use SET SQL_BIG_SELECTS=1 or SET SQL_MAX_JOIN_SIZE=# if the SELECT is okay

There is a certain complex view in our application which apparently hits this default limit set by MySQL once there is a certain amount of data present.  The view itself is a costly one... it really does need to do a whole lot of joins and rewriting it to use less joins would probably take a long time.

So i figured the better option in this case would be to set the SQL_BIG_SELECTS option to 1.  The only problem was: how? It's not a setting that you can pass through the connection string (or at least, i did not find a way to do so) and NHibernate is taking care of all of the database communication.

I remembered a trick i had used <a href="http://davybrion.com/blog/2008/09/extending-nhibernates-driverconnectionprovider/">earlier</a>, which is to extend NHibernate's DriverConnectionProvider.  It could then set the setting with the appropriate value whenever the connection is opened, like this:

<div>

[csharp]
public class CustomConnectionProvider : NHibernate.Connection.DriverConnectionProvider
{
    public override IDbConnection GetConnection()
    {
        IDbConnection connection = base.GetConnection();
        EnableBigSelects(connection);
        return connection;
    }

    private void EnableBigSelects(IDbConnection connection)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = &quot;SET SQL_BIG_SELECTS=1&quot;;
            command.ExecuteNonQuery();
        }
    }
}
[/csharp]
</div>

After that, you just set the DriverConnectionProvider to use in your hibernate.cfg.xml file like this:

<div>
[xml]
&lt;property name=&quot;connection.provider&quot;&gt;MyProject.Infrastructure.NHibernate.CustomConnectionProvider, MyProject&lt;/property&gt;
[/xml]
</div>

And all is well.