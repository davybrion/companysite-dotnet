If you've ever used NHibernate 2.0 or later, you will have undoubtedly run into the following runtime exception a couple of times:

NHibernate.InvalidProxyTypeException: The following types may not be used as proxies:
NHibernateExamples.Entities.OrderLine: method get_UnitPrice should be 'public/protected virtual' or 'protected internal virtual'
NHibernateExamples.Entities.OrderLine: method set_UnitPrice should be 'public/protected virtual' or 'protected internal virtual'

Oops... we forgot to make the UnitPrice property on the OrderLine entity virtual.  But why does it need to be virtual in the first place? That's a question that many people who are new to NHibernate have.

The quick answer to that question is: because we need members to be virtual in order to do our lazy loading magic/voodoo.

The longer answer is more interesting though.  An important feature that any real ORM must have is transparent Lazy Loading.  If you retrieve an object through an ORM, you don't want it to automatically pull in an entire object graph (not by default anyway), yet you don't want to litter your code with checks to see if certain associations have been loaded yet, and then loading them if necessary.  This is the ORM's responsibility.  Ideally, you want to be able to access properties and have the ORM load the necessary data upon first access of those properties if the data hasn't been retrieved yet.

NHibernate has this ability, yet it doesn't require you to inherit from some kind of NHibernate base class or implement any interfaces or anything like that. So how does it work? Well, NHibernate uses proxies of your classes at runtime whenever lazy loading is required.  Ok, so what exactly is a proxy? In this case, an NHibernate proxy is a type which is generated dynamically when NHibernate is initialized for your application (this only happens once upon application startup).  A proxy type will be generated for each of your entities that hasn't explicitly been mapped to avoid lazy loading (more on this later).  A proxy type for one of your entities will actually <strong>inherit</strong> from your entity, and will then intercept each possible call you can perform on that type. 

Let's discuss a small example that might make things clearer.  Suppose you have an Order class.  The Order class has properties such as Employee and Customer, among others.  But when you load Order instances, you might not always want the Employee property to already contain the real Employee entity instance.  Same thing goes for the Customer property.  By default, NHibernate considers each entity type as eligible for lazy loading unless it's been explicitly configured not to (again, more on this later).  So when NHibernate is initialized, it will know that it needs to dynamically generate proxy types for Customer and Employee.  Let's just assume these types will be named CustomerProxyType and EmployeeProxyType (they wouldn't be called like that btw, but it doesn't matter). Now suppose that you are retrieving an Order instance (or a bunch of them, doesn't really matter) and you don't instruct NHibernate to already fetch the Customer or Employee data.  You haven't requested the Customer or Employee data, so it shouldn't be there, right?  But it shouldn't be null either, right?  So NHibernate assigns an instance of the CustomerProxyType class to the Customer property, and an instance of EmployeeProxyType and initializes both proxies so that they contain their identifier value, which you already have in memory anyway after selecting the order record.

You can safely use the Order instance(s) and you can even access the Employee and Customer instances and nothing will happen.  But, whenever you access any of the non-identifier members (that means properties _and_ methods) of a proxy instance, NHibernate needs to make sure that the data of either the Customer or the Employee (depending on which one you're using) needs to be fetched from the database.  So how does NHibernate do that?  The proxies will <strong>override</strong> all of your properties and methods and when one of them is accessed, NHibernate will either fetch the data of the entity if it's not present yet and then proceed with the original implementation of the property or the method, or it will immediately call the original implementation if the data was already present.

This is basic OO... your entities are base classes to NHibernate's proxies, and those proxies need to add a little bit of behavior to your entities' behavior.  In order to do that, NHibernate needs to override every public member to make sure that this extra behavior is triggered at the appropriate time.  Now, there are quite a few people who dislike this requirement.  First of all, there is a minor performance cost to calling virtual members as opposed to calling non virtual members.  However, this performance cost is really extremely small and in practically every situation it's completely negligible.  This extra cost certainly doesn't even compare to some real world performance costs, like hitting the database more often than you should or retrieving more data than you really need.  Another reason why some people don't like this is because they don't like to enable derived classes to override whatever member they want to.  In some cases, this is a valid objection.  In most cases however, it's pure Intellectual Masturbation which offers no real value at all.  There are other ORM's that don't require you to make your members virtual and they are still able to offer lazy loading features.  But those ORM's usually require you to either inherit from a specified base class, or to implement one or more interfaces that the ORM will use.  In both cases, i'd argue that this pollutes your entities far more than virtual members do, but that's just my opinion.

But for those cases where you really do not want to make members virtual, and don't mind forgoing on the lazy-loading features of NHibernate, you can simply map those entities to not enable lazy loading at all.  You could just map an entity like this:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #a31515; }
.cb3 { color: red; }
</style>
<div class="cf">
<p class="cl"><span class="cb1">&nbsp; &lt;</span><span class="cb2">class</span><span class="cb1"> </span><span class="cb3">name</span><span class="cb1">=</span>&quot;<span class="cb1">OrderLine</span>&quot;<span class="cb1"> </span><span class="cb3">table</span><span class="cb1">=</span>&quot;<span class="cb1">OrderLine</span>&quot;<span class="cb1"> </span><span class="cb3">lazy</span><span class="cb1">=</span>&quot;<span class="cb1">false</span>&quot;<span class="cb1"> &gt;</span></p>
</div>
</code>
  
Setting the lazy attribute to false will ensure that NHibernate will not create a proxy type of your entity type, and that you will always be dealing with instances of the actual type of your entity instead of a possible proxy type.  It also means that you will never be able to use any kind of lazy loading when it comes to retrieving instances of these entity types. 




























