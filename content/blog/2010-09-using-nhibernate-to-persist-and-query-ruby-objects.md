As some of you already know, i've been experimenting with getting NHibernate and Ruby (through IronRuby) to play nice together.  In this post, i'll go over what already works and how i got it working.  

Suppose we have the following 2 NHibernate mappings:

<div>
[xml]
  &lt;class entity-name=&quot;Artist&quot;&gt;
    &lt;id name=&quot;id&quot; column=&quot;ArtistId&quot; type=&quot;int&quot;&gt;
      &lt;generator class=&quot;identity&quot;/&gt;
    &lt;/id&gt;

    &lt;property name=&quot;name&quot; length=&quot;50&quot; type=&quot;string&quot; /&gt;

    &lt;bag name=&quot;albums&quot; cascade=&quot;all-delete-orphan&quot; inverse=&quot;true&quot; &gt;
      &lt;key column=&quot;ArtistId&quot;/&gt;
      &lt;one-to-many class=&quot;Album&quot; /&gt;
    &lt;/bag&gt;
  &lt;/class&gt;

  &lt;class entity-name=&quot;Album&quot;&gt;
    &lt;id name=&quot;id&quot; column=&quot;AlbumId&quot; type=&quot;int&quot;&gt;
      &lt;generator class=&quot;identity&quot;/&gt;
    &lt;/id&gt;

    &lt;property name=&quot;title&quot; length=&quot;50&quot; type=&quot;string&quot; not-null=&quot;true&quot; /&gt;
    &lt;many-to-one name=&quot;artist&quot; column=&quot;ArtistId&quot; not-null=&quot;true&quot; class=&quot;Artist&quot; /&gt;
  &lt;/class&gt;
[/xml]  
</div>

And suppose we have the following 2 classes:

<div>
[ruby]
class Artist
  attr_accessor :id, :name, :albums
  
  def initialize
    self.albums = System::Collections::ArrayList.new
  end
  
  def add_album(album)
    self.albums.add(album)
    album.artist = self
  end
  
  def remove_album(album)
    self.albums.remove(album)
    album.artist = nil
  end
end

class Album
  attr_accessor :id, :title, :artist
end
[/ruby]
</div>

The only atypical thing about that Ruby code is the usage of System::Collections::ArrayList.  That's something i haven't been able to workaround yet: if you want to use collections, you'll need to use the .NET ones for now.

I'm relying on 2 things to get everything working.  One is NHibernate's Map EntityMode, the other is my own Ruby magic which i'll cover later.  The important thing to know is that the Map EntityMode basically works without classes, but with dictionaries.  Instead of instances of entity classes, NHibernate will return or accept dictionaries where the keys correspond to property names and the values correspond to their respective property's value.  Though the goal was that the developer need not use the dictionaries directly, as the above 2 Ruby classes show.  I'll get into the details of the Ruby magic later on in this post, but for now it's important to know that there's an ObjectFactory class which takes care of transforming the dictionaries that i get from NHibernate to either real instances of entity classes, or proxies of them.

First, let's take a look at transitive persistence:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		var artist = ruby.Artist.@new();
		artist.name = &quot;Rage Against The Machine&quot;;

		var album1 = ruby.Album.@new();
		album1.title = &quot;Rage Against The Machine&quot;;

		var album2 = ruby.Album.@new();
		album2.title = &quot;Evil Empire&quot;;

		artist.add_album(album1);
		artist.add_album(album2);

		session.Save(&quot;Artist&quot;, artist);

		session.Flush();

		artistId = artist.id();
	}
[/csharp]
</div>

The output of running that code is this:

<div>
[code]
NHibernate: INSERT INTO Artist (name) VALUES (@p0); select SCOPE_IDENTITY();@p0 = 'Rage Against The Machine' [Type: String (50)]
NHibernate: INSERT INTO Album (title, ArtistId) VALUES (@p0, @p1); select SCOPE_IDENTITY();@p0 = 'Rage Against The Machine' [Type: String (50)], @p1 = 355 [Type: Int32 (0)]
NHibernate: INSERT INTO Album (title, ArtistId) VALUES (@p0, @p1); select SCOPE_IDENTITY();@p0 = 'Evil Empire' [Type: String (50)], @p1 = 355 [Type: Int32 (0)]
[/code]
</div>

As you can see, transitive persistence is working nicely, even with collections.  Now let's see how we can retrieve that data from the database and into our Ruby objects.  First i need to show the following 2 helper methods for displaying the data:

<div>
[csharp]
	private static void PrintArtistData(dynamic artist)
	{
		Console.WriteLine(&quot;Artist: &quot; + artist.name());
		PrintAlbumData(artist.albums());
	}

	private static void PrintAlbumData(dynamic albums)
	{
		foreach (dynamic album in albums)
		{
			Console.WriteLine(&quot;\tAlbum: &quot; + album.title());
		}
		Console.WriteLine();
	}
[/csharp]
</div>

Now we can get the artist we just created with a simple call to session.Get:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		dynamic artist = ruby.ObjectFactory.create_from_nhibernate_hash(session.Get(&quot;Artist&quot;, artistId));
		Console.WriteLine(&quot;display output from session.Get&quot;);
		PrintArtistData(artist);
	}
[/csharp]
</div>

And here's the output of that in the console:

<div>
[code]
NHibernate: SELECT artist0_.ArtistId as ArtistId0_0_, artist0_.name as name0_0_ FROM Artist artist0_ WHERE artist0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
display output from session.Get
Artist: Rage Against The Machine
NHibernate: SELECT albums0_.ArtistId as ArtistId1_, albums0_.AlbumId as AlbumId1_, albums0_.AlbumId as AlbumId1_0_, albums0_.title as title1_0_, albums0_.ArtistId as ArtistId1_0_ FROM Album albums0_ WHERE albums0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
        Album: Rage Against The Machine
        Album: Evil Empire
[/code]
</div>

As you can see, the lazy loading of the albums collection works just as you'd expect it to.  Speaking of lazy-loading, we can do the same thing with a call to session.Load instead of session.Get:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		dynamic artist = ruby.ObjectFactory.create_proxy_from_nhibernate_hash(session.Load(&quot;Artist&quot;, artistId), &quot;Artist&quot;, artistId);
		Console.WriteLine(&quot;display output from session.Load&quot;);
		PrintArtistData(artist);
	}
[/csharp]
</div>

As you may or may not know, session.Load returns a proxy of an entity instead of actually fetching it from the database immediately (unless the instance is already in the session cache, which my current ruby code can't handle yet).  NHibernate doesn't hit the database until you access any of the properties of the entity outside of the identifier, which the output of this code clearly shows:

<div>
[code]
display output from session.Load
NHibernate: SELECT artist0_.ArtistId as ArtistId0_0_, artist0_.name as name0_0_ FROM Artist artist0_ WHERE artist0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
Artist: Rage Against The Machine
NHibernate: SELECT albums0_.ArtistId as ArtistId1_, albums0_.AlbumId as AlbumId1_, albums0_.AlbumId as AlbumId1_0_, albums0_.title as title1_0_, albums0_.ArtistId as ArtistId1_0_ FROM Album albums0_ WHERE albums0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
        Album: Rage Against The Machine
        Album: Evil Empire
[/code]
</div>

Notice that the select statement is outputted right before we access the name of the artist, instead of immediately as in the previous example.  

We've got lazy-loading covered, but what about eager loading? Well, take a look at the following code:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		var artistHash = session.CreateCriteria(&quot;Artist&quot;)
			.Add(Restrictions.IdEq(artistId))
			.SetFetchMode(&quot;albums&quot;, FetchMode.Join)
			.List()[0];

		dynamic artist = ruby.ObjectFactory.create_from_nhibernate_hash(artistHash);
		Console.WriteLine(&quot;display output from session.CreateCriteria without any lazy loading&quot;);
		PrintArtistData(artist);
	}
[/csharp]
</div>

This fetches our artist and immediately joins its albums in the same query.  When we access the albums of the artist, it no longer needs to go to the database:

<div>
[code]
NHibernate: SELECT this_.ArtistId as ArtistId0_1_, this_.name as name0_1_, albums2_.ArtistId as ArtistId3_, albums2_.AlbumId as AlbumId3_, albums2_.AlbumId as AlbumId1_0_, albums2_.title as title1_0_, albums2_.ArtistId as ArtistId1_0_ FROM Artist this_ left outer join Album albums2_ on this_.ArtistId=albums2_.ArtistId WHERE this_.ArtistId = @p0;@p0 = 355 [Type: Int32 (0)]
display output from session.CreateCriteria without any lazy loading
Artist: Rage Against The Machine
        Album: Rage Against The Machine
        Album: Evil Empire
[/code]
</div>

Obviously, if we omit setting the fetchmode of the albums association we get the same output as we would get from using session.Get:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		var artistHash = session.CreateCriteria(&quot;Artist&quot;)
			.Add(Restrictions.IdEq(artistId))
			.List()[0];

		dynamic artist = ruby.ObjectFactory.create_from_nhibernate_hash(artistHash);
		Console.WriteLine(&quot;display output from session.CreateCriteria with lazy loading of albums&quot;);
		PrintArtistData(artist);
	}
[/csharp]
</div>

<div>
[code]
NHibernate: SELECT this_.ArtistId as ArtistId0_0_, this_.name as name0_0_ FROM Artist this_ WHERE this_.ArtistId = @p0;@p0 = 355 [Type: Int32 (0)]
display output from session.CreateCriteria with lazy loading of albums
Artist: Rage Against The Machine
NHibernate: SELECT albums0_.ArtistId as ArtistId1_, albums0_.AlbumId as AlbumId1_, albums0_.AlbumId as AlbumId1_0_, albums0_.title as title1_0_, albums0_.ArtistId as ArtistId1_0_ FROM Album albums0_ WHERE albums0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
        Album: Rage Against The Machine
        Album: Evil Empire
[/code]
</div>

Eager fetching also works in the other direction, when fetching albums with their artist included automatically:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		var albumsList = session.CreateCriteria(&quot;Album&quot;)
			.CreateAlias(&quot;artist&quot;, &quot;a&quot;, JoinType.InnerJoin)
			.SetMaxResults(5)
			.List();

		dynamic albums = ruby.ObjectFactory.create_multiple_from_nhibernate_list(albumsList);

		foreach (dynamic album in albums)
		{
			Console.WriteLine(string.Format(&quot;'{0}' by '{1}'&quot;, album.title(), album.artist().name()));
		}
	}
[/csharp]
</div>

This results in the following output:

<div>
[code]
NHibernate: SELECT TOP (@p0) this_.AlbumId as AlbumId1_1_, this_.title as title1_1_, this_.ArtistId as ArtistId1_1_, a1_.ArtistId as ArtistId0_0_, a1_.name as name0_0_ FROM Album this_ inner join Artist a1_ on this_.ArtistId=a1_.ArtistId;@p0 = 5 [Type: Int32 (0)]
'For Those About To Rock We Salute You 2' by 'Accept'
'Balls to the Wall' by 'Accept'
'Restless and Wild' by 'Accept'
'Let There Be Rock' by 'AC/DC'
'Big Ones' by 'Aerosmith'
[/code]
</div>

Finally, we'll retrieve our artist and modify some of its data:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		dynamic artist = ruby.ObjectFactory.create_from_nhibernate_hash(session.Get(&quot;Artist&quot;, artistId));

		artist.name = &quot;RATM&quot;;
		artist.albums()[1].title = &quot;The Battle Of Los Angeles&quot;;

		artist.remove_album(artist.albums()[0]);

		dynamic newAlbum = ruby.Album.@new();
		newAlbum.title = &quot;Renegades&quot;;
		artist.add_album(newAlbum);

		session.Flush();
	}
[/csharp]
</div>

If we then run the following code again:

<div>
[csharp]
	using (var session = sessionFactory.OpenSession())
	{
		dynamic artist = ruby.ObjectFactory.create_from_nhibernate_hash(session.Get(&quot;Artist&quot;, artistId));
		Console.WriteLine(&quot;display output from session.Get&quot;);
		PrintArtistData(artist);
	}
[/csharp]
</div>

We can see that the data has indeed been changed as it should:

<div>
[code]
NHibernate: SELECT artist0_.ArtistId as ArtistId0_0_, artist0_.name as name0_0_ FROM Artist artist0_ WHERE artist0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
display output from session.Get
Artist: RATM
NHibernate: SELECT albums0_.ArtistId as ArtistId1_, albums0_.AlbumId as AlbumId1_, albums0_.AlbumId as AlbumId1_0_, albums0_.title as title1_0_, albums0_.ArtistId as ArtistId1_0_ FROM Album albums0_ WHERE albums0_.ArtistId=@p0;@p0 = 355 [Type: Int32 (0)]
        Album: The Battle Of Los Angeles
        Album: Renegades
[/code]
</div>

Ok, so how does this all work? After all, NHibernate returns and expects dictionaries and as you can see in the code of the ruby classes, there are no dictionaries being used.  The answer is actually pretty simple.  NHibernate returns and expects dictionaries.  I return and expect entity instances.  Clearly, all we need to do is make sure that our entities pretend to be dictionaries and NHibernate will never need to know what on earth we're doing.

The first thing we need to do is to modify the implementation of the ruby classes that we have created for our entities.  Obviously, i wouldn't want anyone to have to do that manually, so my ruby magic just does this at runtime.  The only limit that is placed on the code you write in ruby is that within the entity classes, you can never touch the private instance fields of the attributes that you've defined.  You always have to go through the accessors.  Because of that limit, i can just replace all of the accessor methods with implementations that use the dictionary that NHibernate gives me as the backing store of the data instead of using instance fields.  I also make sure that all equality checks are based on the underlying dictionary instead of the actual object.  This passes everything but a straight-up reference check.  Finally, we need to make sure that our objects can be cast to an IDictionary and that we implement the indexer property of the IDictionary interface because NHibernate will use that when we pass it transient instances to insert into the database.

First, let's take a look at the ObjectFactory class, which has a couple of class methods that we use from our .NET code to create entities based on the dictionaries that we get from NHibernate:

<div>
[ruby]
class ObjectFactory
  def self.create_from_nhibernate_hash(hashtable)
    entity_name = hashtable[NHibernator::TYPE_KEY_NAME.to_clr_string]
    entity = const_get(entity_name.to_sym).new
    entity.hydrate_from hashtable
    entity    
  end
  
  def self.create_proxy_from_nhibernate_hash(hashtable, entity_name, id)
    proxy = const_get(&quot;#{entity_name}Proxy&quot;.to_sym).new
    proxy.hydrate_from hashtable, id
    proxy
  end
  
  def self.create_multiple_from_nhibernate_list(list)
    entities = []
    # TODO: differentiate between proxies and normal entities in the list
    list.each { |hash| entities &lt;&lt; create_from_nhibernate_hash(hash) }
    entities
  end
end
[/ruby]
</div>

(as you can see from the TODO statement, this whole thing is still a work in progress)

Pretty simple stuff so far... We either create a new instance of the entity class, or of a proxy class for that entity type (i'll cover the creation of proxy classes soon).  We then call its hydrate_from method, which is also added to each entity class dynamically.  There's another (temporary) limitation here... i search for the class name constant in Object, which means that our current approach doesn't work when our entities have namespaces.  Not really a problem for this example, and is easy to add later on when i actually need it.  That's it for the ObjectFactory... the real magic is all contained in the NHibernator module.  And no, i couldn't come up with a better name.  Long-time readers should know by now that i absolutely suck at coming up with good names so that's why we ended up with the NHibernator module.

The NHibernator module does 2 things: it offers a method that you need to use when initializing your application so we can create the proxy classes based on NHibernate's metadata, and it also modifies the accessor methods and adds some new methods whenever it is mixed in to another class.  I'm going to show the code of the NHibernator module in multiple steps to hopefully keep everything as clear as possible.  First of all, i'm gonna show the declaration of a constant and a simple helper method that we're going to need:

<div>
[ruby]
  TYPE_KEY_NAME = &quot;$type$&quot;

  def self.each_writeable_accessor_of(klass, &amp;block)
    setters = klass.public_instance_methods(true).select { |name| name =~ /\w=$/ }
    setters.each { |setter| yield setter }
  end  
[/ruby]
</div>

The TYPE_KEY_NAME constant contains the string that NHibernate uses as the key in its dictionaries for the value which returns the current entity's type name.  And the each_writeable_accessor_of method executes the given block for each writeable acessor that a class contains.

And this is how we initialize everything:

<div>
[ruby]
  def self.initialize(session_factory)
    all_class_metadata = session_factory.get_all_class_metadata
    
    all_class_metadata.keys.each do |key|
      metadata = all_class_metadata[key]
      realclass = Object::const_get(key)
      realclass.send :include, NHibernator
      create_proxy_class_for realclass, metadata.identifier_property_name
    end
  end
  
  def self.create_proxy_class_for(klass, identifier_name)
    proxyclass = Class.new(klass)
    Object::const_set &quot;#{klass.name}Proxy&quot;, proxyclass
    
    proxyclass.class_eval do
      define_method identifier_name do
        @id
      end
      
      def hydrate_from(hashtable, id)
        @nhibernate_values = hashtable
        @id = id
      end
    end
    
    each_writeable_accessor_of(klass) do |setter|
      proxyclass.class_eval do
        define_method setter do |value|
          # execute the getter to force NH's lazy proxymap to fetch the data
          send setter.to_s.chop
          super value
        end
      end
    end
  end
[/ruby]
</div>

The initialize class method takes an NHibernate ISessionFactory instance and retrieves each mapped entity with the information that we need about it.  Each mapped entity's class is sent the include message with the NHibernator module as a parameter.  This basically mixes in the functionality of the NHibernator module into each entity's class.  I'll discuss this in the next part of the post.  After we've mixed the module into the entity classes, we call the create_proxy_class_for method for each class.  As you can see, creating the proxy classes is very easy stuff.  Any proxy class that we create inherits from the class of the entity, and overrides the accessor method to retrieve the identifier value so that it immediately returns the identifier value.  If we would've kept the default implementation, it would access the dictionary that we got from NHibernate, which would cause a select statement for this proxy to be issued, which we obviously don't want.  Again, this is a work in progress and one limitation that this current proxy implementation has is that you'll get a reference to a dictionary instead of an entity when you access a reference-property of a proxy.  That too will be easy to fix :)

Next up, we need to cover what happens when the NHibernator module is mixed into an entity class.  Ruby has a great hook method for that, which is this:

<div>
[ruby]
  def self.include(base)
    # everything you do within this method will be executed whenever
    # this module is included in a class... the base parameter is
    # the class that included the module
    
    # ...
  end
[/ruby]
</div>

I'm doing quite a bit within that method and i want to cover each item in detail.  So, the next couple of pieces of code are all part of the self.include(base) method implementation.  The first thing we do when this module gets included in a class is this:

<div>
[ruby]
    each_writeable_accessor_of(base) do |setter|
      getter = setter.to_s.chop
      
      base.class_eval do
        undef_method getter
        undef_method setter
        
        define_method getter do
          return nil if @nhibernate_values.nil?
          value = @nhibernate_values[getter.to_clr_string]
          return value unless value.is_a? System::Collections::IEnumerable
          # TODO: cache the WrappedList instance
          WrappedList.new(value)
        end
        
        define_method setter do |value|
          @nhibernate_values = System::Collections::Hashtable.new if @nhibernate_values.nil?
          @nhibernate_values[setter.to_s.chop.to_clr_string] = value
        end
      end
    end
[/ruby]
</div>

This is pretty simple, we're just getting rid of all of the original accessor methods and replacing them with our own implementations that use the dictionary we get from NHibernate as the backing store.  Note that i will discuss the WrappedList class that you see in those getters soon.  The setter methods will also instantiate a new Hashtable if we don't already have a dictionary.  This is necessary for transient instances since NHibernate will treat them as IDictionary instances when we pass them to the session.  Speaking of which, this is the next thing we do:

<div>
[ruby]
    base.send :include, System::Collections::IDictionary
[/ruby]
</div>

This single line enables any piece of .NET code to cast our instances to an IDictionary reference.  Note that we haven't even implemented any of the IDictionary interface's methods yet.  We don't need to implement all of them anyway, just the ones that we know will be used.

Finally, we add all of the following methods to each class that included this module:

<div>
[ruby]
    base.class_eval do
      def nhibernate_values
        @nhibernate_values
      end

      def hydrate_from(hashtable)
        @nhibernate_values = hashtable
        referenced_entities = Hash.new

        hashtable.keys.each do |key|
          value = hashtable[key.to_clr_string]
                    
          if value.is_a? System::Collections::IDictionary
            if value.is_a? System::Collections::Hashtable
              referenced_entity = ObjectFactory.create_from_nhibernate_hash(value)
            else
              type = value.hibernate_lazy_initializer.entity_name
              id = value.hibernate_lazy_initializer.identifier
              referenced_entity = ObjectFactory.create_proxy_from_nhibernate_hash(value, type, id)
            end
          
            referenced_entities[key] = referenced_entity
          end
        end

        referenced_entities.keys.each { |key| send &quot;#{key}=&quot;, referenced_entities[key] }
      end
      
      def Equals(other)
        self == other
      end

      def GetHashCode
        hash
      end
            
      def ==(other)
        return false if other.nil?
        return @nhibernate_values.Equals(other) if other.is_a? System::Collections::IDictionary
        return false unless other.respond_to? :nhibernate_values
        other.nhibernate_values.Equals(@nhibernate_values)        
      end
            
      def hash
        @nhibernate_values.GetHashCode()
      end
                                                
      def [](key)
        self.send key
      end 

      def []=(key, value)
        self.send &quot;#{key}=&quot;, value
      end      
    end
[/ruby]
</div>

I think that code speaks for itself, except for the Equals and GetHashCode methods... those are just there because i had some issues with IronRuby mapping calls to Equals or GetHashCode to their corresponding ruby alternatives (== and hash).  I eventually upgraded to the latest IronRuby revision from GitHub, because i didn't get correct results with the IronRuby 1.1 alpha 1 to get the equality checks working correctly.

Finally, i needed the following 2 helper classes to make the albums bag work correctly:

<div>
[ruby]
class WrappedList
  include System::Collections::IList
  
  def initialize(list)
    @list = list  
  end
  
  def each(&amp;block)
    @list.each do |item|
      if item.respond_to? :nhibernate_values
        yield item
      else
        yield ObjectFactory.create_from_nhibernate_hash(item)
      end
    end
  end

  def GetEnumerator
    WrappedListEnumerator.new(self)
  end

  def add(item)
    @list.add item
  end
  
  def clear
    @list.clear
  end
  
  def contains(item)
    @list.contains item
  end
  
  def count
    @list.count
  end
  
  def remove(item)
    original_count = count
    if item.respond_to? :nhibernate_values
      @list.remove item.nhibernate_values
    else
      @list.remove item
    end
    original_count != count
  end
  
  def is_read_only
    @list.is_read_only
  end
  
  def index_of(item)
    @list.index_of item
  end
  
  def insert(index, item)
    @list.insert index, item
  end
  
  def remove_at(index)
    @list.remove_at index
  end
  
  def [](index)
    item = @list[index]
    return item if item.respond_to? :nhibernate_values
    ObjectFactory.create_from_nhibernate_hash(item)
  end
  
  def []=(index, item)
    @list[index] = item
  end
  
  def Equals(other)
    self == other
  end

  def GetHashCode
    hash
  end
            
  def ==(other)
    return false if other.nil?
    @list.Equals(other)
  end
        
  def hash
     GetHashCode
  end
end

class WrappedListEnumerator
  include System::Collections::IEnumerator
  
  def initialize(wrappedlist)
    @wrappedlist = wrappedlist
    reset
  end
  
  def reset
    @current_index = -1
  end
  
  def current
    @wrappedlist[@current_index]
  end
  
  def move_next
    @current_index += 1
    return false if @current_index &gt;= @wrappedlist.count
    true
  end
end
[/ruby]
</div>

And that's all there is to it.  This is probably the longest blog post i've ever written, but the amount of code involved in getting this working really isn't that much.  Granted, there are still limitations to this approach so some stuff will need to be added to it.  I'm also not saying that this is actually a great idea or that you should start doing this from now on, but well, at least this is possible now :)