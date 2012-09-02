Creating a DSL seems like a hard thing to do, right? While there are various interesting challenges that you'll need to deal with if you want to build and use a real DSL, the initial step of getting it working is actually a lot easier than you might think it is.  I'm gonna walk you through the creation of a simple DSL, suitable for a domain that all of us have experience with.  The domain is quite simple: describing entities, their properties and their relationships.  Forget for a second that you could obviously get the exact same information from a set of class definitions.  It's merely a technical exercise using a domain that we all know :)

One of the key questions that we need to ask ourselves is: what kind of concepts do we want to be able to describe with our DSL? In our case, we want to describe a model consisting of entities.  Disregarding behavior for now, we can say that each entity will consist of properties.  Properties could be regular properties, references to other Entities, or collections of other entities.  Here's one way (of many, obviously) to model that in Ruby (since that language makes it very easy to define a DSL):

<div>
[ruby]
require 'forwardable'

class Model
  extend Forwardable
  
  def initialize
    @entities = []
  end

  def add_entity(entity)
    @entities &lt;&lt; entity
  end
  
  def entity_for(name)
    @entities.detect { |entity| entity.name == name }
  end

  def_delegator :@entities, :each, :each_entity  
end

class Entity
  extend Forwardable

  attr_reader :name

  def initialize(name)
    @name = name
    @collections = []
    @properties = []
    @references = []
  end
  
  def add_collection(collection)
    @collections &lt;&lt; collection
  end
  
  def add_property(property)
    @properties &lt;&lt; property
  end
  
  def add_reference(reference)
    @references &lt;&lt; reference
  end
  
  def identifier
    @properties.detect { |property| property.is_identifier? }
  end

  def_delegator :@collections, :each, :each_collection
  def_delegator :@properties, :each, :each_property
  def_delegator :@references, :each, :each_reference  
end

class Property
  attr_reader :name
  attr_reader :required
  attr_reader :type
  
  def self.new_identifier(name, type)
    self.new(name, type, false, true)
  end
  
  def initialize(name, type, required=false, is_identifier=false)
    @name = name
    @type = type
    @required = required
    @is_identifier = is_identifier
  end
  
  def is_identifier?
    @is_identifier
  end
end

class Reference
  attr_reader :name
  attr_reader :entity
  attr_reader :is_required
  
  def initialize(name, entity, is_required)
    @name = name
    @entity = entity
    @is_required = is_required
  end
end

class Reference
  attr_reader :name
  attr_reader :entity
  attr_reader :is_required
  
  def initialize(name, entity, is_required)
    @name = name
    @entity = entity
    @is_required = is_required
  end
end

class Collection
  attr_reader :name
  attr_reader :entity

  def initialize(name, entity)
    @name = name
    @entity = entity
  end
end
[/ruby]
</div>

That gives us a simple, yet complete object model to describe entities, their properties and their relationships.  The next question is: how do we define the DSL?  Considering an Invoice entity, suppose we'd like to describe it in our DSL like this:

<div>
[ruby]
entity &quot;Invoice&quot;
identified_by &quot;Id&quot;, :guid
must_reference &quot;Customer&quot;
must_have &quot;Date&quot;, :date
can_have &quot;Discount&quot;, :double
contains &quot;Lines&quot;, &quot;InvoiceLine&quot;
[/ruby]
</div>

This really tells us anything we need to know about this entity, and we could use this data for pretty much everything we want.  There is no explicit or implicit link to any specific kind of technology, like say, a relational database, a document database, or some kind of databinding technology.  We could transform or extend this data to suit whichever purpose we deem fit.

So now that we know how we want to describe our entities and their relationships, we can implement the language.  As mentioned in the title of this post, this is the implementation of a <em>simple</em> DSL.  It's just to illustrate an idea, and not an approach that is guaranteed to stand up to the real-world requirements that a DSL could face (and that depends on a case by case basis). So in this case, we're going to go with an implementation where each entity is described in its own file, and its filename must end with '_def.rb'.  With that limitation in mind, we can do this:

<div>
[ruby]
require_relative 'application.rb'
require_relative 'entity.rb'
require_relative 'collection.rb'
require_relative 'property.rb'
require_relative 'reference.rb'

@model = Model.new

def entity(name)
  entity = Entity.new(name)
  @model.add_entity entity
  @current_entity = entity
end

def identified_by(name, type)
  @current_entity.add_property Property.new_identifier(name, type)
end

def must_have(name, type)
  has name, type, true
end

def can_have(name, type)
  has name, type, false
end

def must_reference(entity_name, name=nil)
  references entity_name, true, name
end

def can_reference(entity_name, name=nil)
  references entity_name, false, name
end

def contains(name, entity_name)
  referred_entity = @model.entity_for entity_name
  @current_entity.add_collection Collection.new(name, referred_entity)
end

def has(name, type, required=false)
  @current_entity.add_property Property.new(name, type, required)
end

def references(entity_name, is_required=false, name=nil)
  referred_entity = @model.entity_for entity_name
  name = entity_name if name.nil?
  @current_entity.add_reference Reference.new(name, entity_name, is_required)
end

Dir.glob('*_def.rb').each do |file| 
  @current_entity = nil
  load file
end

@model.each_entity do |entity|
  puts entity.name
  print_name = Proc.new { |item| puts &quot;\t\t#{item.name}&quot;}
  puts &quot;\t has the following properties:&quot;
  entity.each_property &amp;print_name
  puts &quot;\t has the following references:&quot;
  entity.each_reference &amp;print_name
  puts &quot;\t has the following collections:&quot;
  entity.each_collection &amp;print_name
end
[/ruby]
</div>

As you can see, we have 'global' method definitions (they're actually implicitly added to the Object class) which correspond with our language 'keywords'.  Those method implementations use the model that we defined earlier to build a nice object graph based on what we describe through our DSL.

You'll notice that after the method definitions, you can see the following code:

<div>
[ruby]
Dir.glob('*_def.rb').each do |file| 
  @current_entity = nil
  load file
end
[/ruby]
</div>

And that's the clue to this simple DSL: it loops through each file that matches the '*_def.rb' pattern, sets an instance variable named @current_entity (implicitly added to the current Object instance in this case) to nil, and then <em>loads</em> the current file in the loop.  The load method (it might look like a keyword, but it's a method) executes the ruby code in the given file <em>in place</em>, meaning that it shares the same scope.  In other words, the methods that we've defined here are accessible to our DSL declarations since those are executed within the same scope.  And since those method implementations manipulate our domain model, we just built a simple 'language' to describe our entities, their properties and their relations.

Suppose we've got the following entity definitions (each would be in a separate file, but they are just listed all at once here):

<div>
[ruby]
entity &quot;Customer&quot;
identified_by &quot;Id&quot;, :guid
must_have &quot;Name&quot;, :string
can_have &quot;Email&quot;, :string

entity &quot;Product&quot;
identified_by &quot;Id&quot;, :guid
must_have &quot;Name&quot;, :string
must_have &quot;Price&quot;, :integer

entity &quot;InvoiceLine&quot;
identified_by &quot;Id&quot;, :guid
must_reference &quot;Product&quot;
must_have &quot;Count&quot;, :integer

entity &quot;Invoice&quot;
identified_by &quot;Id&quot;, :guid
must_reference &quot;Customer&quot;
must_have &quot;Date&quot;, :date
can_have &quot;Discount&quot;, :double
contains &quot;Lines&quot;, &quot;InvoiceLine&quot;
[/ruby]
</div>

This describes a very small domain model consisting of 4 entities.  In the code listed above, you may have noticed the following piece at the end:

<div>
[ruby]
@model.each_entity do |entity|
  puts entity.name
  print_name = Proc.new { |item| puts &quot;\t\t#{item.name}&quot;}
  puts &quot;\t has the following properties:&quot;
  entity.each_property &amp;print_name
  puts &quot;\t has the following references:&quot;
  entity.each_reference &amp;print_name
  puts &quot;\t has the following collections:&quot;
  entity.each_collection &amp;print_name
end
[/ruby]
</div>

Given the 4 described entities, running the code above results in the following output:

<div>
[code]
Customer
	 has the following properties:
		Id
		Name
		Email
	 has the following references:
	 has the following collections:
Invoice
	 has the following properties:
		Id
		Date
		Discount
	 has the following references:
		Customer
	 has the following collections:
		Lines
InvoiceLine
	 has the following properties:
		Id
		Count
	 has the following references:
		Product
	 has the following collections:
Product
	 has the following properties:
		Id
		Name
		Price
	 has the following references:
	 has the following collections:
[/code]
</div>

So there you have it, we described our entities, their properties and their relationships in a very simple manner and those descriptions were interpreted and the data has been put into an object model that we can use for a variety of purposes if we wanted to.  And there really are a lot of interesting things we can do with this, especially when keeping IronRuby in mind :)