(NOTE: i'm still learning since all of this stuff is very new to me, so everything i say here should be taken with a grain of salt. Better solutions/approaches than what i'm doing are likely available and i'd definitely appreciate good tips from anyone who reads this.)

I started working on a breakable toy, and the first thing i wanted to get working was storing my documents in <a href="http://www.mongodb.org/">MongoDB</a> and getting them back out again. I first read Karl Seguin's <a href="http://openmymind.net/mongodb.pdf">The Little MongoDB Book</a> because it's a great introduction to MongoDB and to working with document databases in general (btw: it's free, only 33 pages and very well written => highly recommended!). 

I originally intended to use MongoDB directly in my code, but once i realized that i wouldn't be able to simply create entity types with their own behavior and just store them in the database, i looked for alternatives. The 'problem' is that MongoDB just expects documents, which are JavaScript objects. But if you pass it a JavaScript object with functions, it'll just store those functions as strings. Not exactly what you want. You can move the functions to the prototype of the object, and if you have more than a few instances that's almost always better than storing the functions in the object itself, but MongoDB doesn't care about the prototype and doesn't return objects with their original prototype when you execute queries. There is a rather ugly trick that you can use to change the prototype of the object (settings its __proto__ property) but it's not official JavaScript and even though Node itself relies on that ability, it felt like too much of a hack to use it. I also didn't feel like writing a layer of infrastructure code on top of the MongoDB driver because then i'd just spend too much time working on infrastructure stuff which i was hoping to avoid on this project.

So i looked around and found <a href="http://mongoosejs.com/">Mongoose</a>. It presents itself as an ORM-like layer on top of MongoDB which it kinda is, though i prefer to think of it as an Object Document Mapper instead of an Object Relational Mapper since they definitely differ in significant ways. Anyways, let's get to some code. As mentioned in the first post about my toy project, the goal is to generate invoices and timesheets for the work i do for my customers. So i started off with 2 entities (obviously, i'll need more, but this is enough to get started):

<div>
[javascript]
	var mongoose = require('mongoose'),
		Schema = mongoose.Schema,
		ObjectId = Schema.ObjectId;
		
	var customerSchema = new Schema({
		name: { type: String, required: true },
		address: {
			street: { type: String, required: true },
			postalCode: { type: String, required: true },
			city: { type: String, required: true },
			country: String
		},
		phoneNumber: String,
		vatNumber: { type: String, required: true },
		contact: {
			name: String,
			email: String
		},
		includeContactOnInvoice: { type: Boolean, required: true, default: false }
	}); 

	mongoose.model('Customer', customerSchema);
	var Customer = mongoose.model('Customer');

	var performedWorkSchema = new Schema({
		date: { type: Date, required: true },
		hours: { type: Number, min: 1, max: 8, required: true }
	});
	
	mongoose.model('PerformedWork', performedWorkSchema);
	var PerformedWork = mongoose.model('PerformedWork');
	
	var activitySchema = new Schema({
		customer: { type: ObjectId, required: true },
		description: { type: String, required: true },
		hourlyRate: { type: Number, required: true },
		performedWork: [performedWorkSchema],
		billed: { type: Boolean, required: true, default: false }
	});

	activitySchema.methods.addPerformedWork = function(date, hours) {
		this.performedWork.push(new PerformedWork({ date: date, hours: hours }));
	};
	
	mongoose.model('Activity', activitySchema);
	var Activity = mongoose.model('Activity');
[/javascript]
</div>

There's quite a bit going on in this piece of code already. This defines the schema of our entities. Some of you might be thinking "wait a sec, i thought document databases were schema-less?". They are indeed, but Mongoose uses these 'Schema' instances to generate constructor functions for your entity objects and to give them some interesting behavior out of the box. Those schema-definitions in the code are still meaningless as far as MongoDB is concerned. But for Mongoose and our code, they certainly are important.

As you can see, we define a Customer type which has some properties, as well as embedded Address and Contact objects. When properties are made required or given default values, it only has an influence on Mongoose. I could still connect to MongoDB through its shell (which is awesome btw, check Karl Seguin's book for some interesting examples) and insert whatever i want in the collections (similar to tables in a relational database, though there is no schema that is upheld for the elements in the collection). We also have a PerformedWork type, though there won't be a collection in the database for those instances. You can see in the schema of Activity that its performedWork property holds an array of PerformedWork instances. We just mapped a one-to-many without requiring a separate MongoDB Collection (or table, if you prefer to think of it that way). If you're using MongoDB directly, you could just put whatever you want in an array-property of a document. For Mongoose, it's important to know the structure of the data, so you have to define a schema for embedded documents in arrays. Notice also that i can easily define min and max values for the hours property of PerformedWork. You can go a lot further with validation in your entity objects, but i haven't looked further into that yet. Also interesting to note is that Activity has a customer property, in which we'll store an ObjectId. It means that the customer property will refer to a customer through the id value that it holds, but it is <em>not</em> an actual Customer reference. I'll show you how the actual documents are stored in the database later on in this post.

Another thing that you'll probably find weird is this:

<div>
[javascript]
	mongoose.model('Customer', customerSchema);
	var Customer = mongoose.model('Customer');
[/javascript]
</div>

The first line basically tells Mongoose that there is a 'Customer' type and the passed in schema instance is the one that Customer instances should be based on. The second call to mongoose.model actually returns a constructor function which we can use to create our types or to execute queries through methods on the constructor function. Sounds weird, but in JavaScript a function is a first class object, which means that a function can have properties as well, and those properties can in turn contain other functions. Think of it as static methods... it's not entirely the same, but it's sort of similar, sometimes. I'll try to make the rest of this post less confusing than this paragraph was :)

To make sure that my entity objects can be stored in MongoDB and retrieved again, i wanted to write some automated tests. While there are a few good testing frameworks for JavaScript, i'm going to (try to) use <a href="http://pivotal.github.com/jasmine/">Jasmine</a> exclusively, through <a href="https://github.com/mhevery/jasmine-node">Jasmine-node</a> (which just runs the tests on Node). Jasmine is a BDD framework, so a lot of people probably wouldn't use it for more technical tests or exploratory tests, but i'm testing technical <em>behavior</em> here so i think it still fits.

Suppose i want to test some of the behavior related to saving a customer, i'd start with something like this:

<div>
[javascript]
describe('when a customer is saved', function() {

});
[/javascript]
</div>

Within the function that is passed to the describe method, i can start adding some tests. For instance, here's one that tests whether or not Mongoose applies the validation rules i specified on my CustomerSchema:

<div>
[javascript]
	describe('with none of its required fields filled in', function() {
		it('should fail with validation errors for each required field', function() {
			var customer = new Customer();
			customer.save(function(err) {
				expect(err).not.toBeNull();
				expect(err).toHaveRequiredValidationErrorFor('name');
				expect(err).toHaveRequiredValidationErrorFor('vatNumber');
				expect(err).toHaveRequiredValidationErrorFor('address.street');
				expect(err).toHaveRequiredValidationErrorFor('address.postalCode');
				expect(err).toHaveRequiredValidationErrorFor('address.city');
				asyncSpecDone();
			});
			asyncSpecWait();
		});
	});
[/javascript]
</div>

This test tries to save an empty customer object to the database, but our CustomerSchema specifies that a couple of its properties are required. Our customer object has a save method (created by Mongoose), and we need to pass it a callback which will be executed after the customer has been inserted. On Node, all I/O calls are asynchronous so you have to tell Jasmine-node to wait for the callback to executed, which is what the call to asyncSpecWait() does. When we get in our callback, we assert that the passed in error object (typically named 'err') is not null, and then we use the toHaveRequiredValidationErrorFor method to assert whether the expected validation error messages are present. The toHaveRequiredValidationErrorFor method doesn't come with Jasmine, it's a custom matcher which we make available before each test:

<div>
[javascript]
beforeEach(function() {
	this.addMatchers((function() {
		var toHaveValidationErrorFor = function(err, validatorName, propertyName) {
			if (!err) { return false; }
			if (err.name !== 'ValidationError') { return false; }
			var value = err.errors[propertyName];
			if (!value) { return false; }
			return (value === 'Validator &quot;' + validatorName + '&quot; failed for path ' + propertyName);
		};
				
		return {
			toHaveRequiredValidationErrorFor : function(propertyName) {
				return toHaveValidationErrorFor(this.actual, 'required', propertyName);
			},
			toHaveMaxValidationErrorFor: function(propertyName) {
				return toHaveValidationErrorFor(this.actual, 'max', propertyName);
			}
		};
	}()));
});
[/javascript]
</div>

We pass an object containing 2 functions to the addMatchers function, which will in turn make those 2 methods available to our expectations.

Let's take a look at a more interesting example, saving an Activity object with an array of PerformedWork instances:

<div>
[javascript]
	describe('with valid performed work added to it', function() {
		it('should be inserted as well', function() {
			var activity = new ActivityBuilder().build();
			var today = new Date();
			var yesterday = new Date();
			yesterday.setDate(yesterday.getDate() -1);
			activity.addPerformedWork(yesterday, 8);
			activity.addPerformedWork(today, 6);
			activity.save(function(err) {
				expect(err).toBeNull();
				Activity.findById(activity.id, function(err, result) {
					expect(result.performedWork.length).toBe(2);
					expect(result.performedWork[0].date).toEqual(yesterday);
					expect(result.performedWork[0].hours).toEqual(8);
					expect(result.performedWork[1].date).toEqual(today);
					expect(result.performedWork[1].hours).toEqual(6);
					asyncSpecDone();
				});
			});
			asyncSpecWait();
		});
	});
[/javascript]
</div>

The ActivityBuilder constructor constructs a typical builder object. I won't go into the details of this pattern, and i won't list the code since this post is already getting a bit too long but you can look at the code <a href="https://github.com/davybrion/therabbithole/blob/master/spec/builders/activity_builder.js">here</a> if you're interested. Anyways, back to the test. We're creating an activity object and using the addPerformedWork function (which we added to ActivitySchema.methods in the first code snippet) to add some performed hours to the activity. We expect the save function to not cause errors, and then we launch a simple query: finding an activity by its id value. Note how we use the findById function through the Activity <em>variable</em>. That Activity variable points to the constructor function which creates activity instances when invoked directly, but as i mentioned earlier it can have properties, which can hold functions, of its own. Notice the syntactic similarity with calling a static method in a static language. Behind the scenes it's entirely different, but from a conceptual point of view, it's sorta the same. As you can see, in this test the save operation works, so what does the activity object, or better yet, document look like in the database? Here it is:

<div>
[javascript]
{ 
	&quot;performedWork&quot; : [
		{
			&quot;_id&quot; : ObjectId(&quot;4e25f7d2041ec8c006000006&quot;),
			&quot;date&quot; : ISODate(&quot;2011-07-18T21:32:02.652Z&quot;),
			&quot;hours&quot; : 8
		},
		{
			&quot;_id&quot; : ObjectId(&quot;4e25f7d2041ec8c006000008&quot;),
			&quot;date&quot; : ISODate(&quot;2011-07-19T21:32:02.652Z&quot;),
			&quot;hours&quot; : 6
		}
	], 
	&quot;billed&quot; : false, 
	&quot;_id&quot; : ObjectId(&quot;4e25f7d2041ec8c006000005&quot;), 
	&quot;hourlyRate&quot; : 75, 
	&quot;description&quot; : &quot;some cool project&quot;, 
	&quot;customer&quot; : ObjectId(&quot;4e25937456436de850000006&quot;) 
}
[/javascript]
</div>

It doesn't actually store it with all that whitespace, i just formatted it to increase readability. Anyways, what's interesting here is that we have our array of PerformedWork instances embedded right here in our document.  So whenever we retrieve this activity instance, we automatically get its PerformedWork instances as well. Also notice the _id properties. We never defined id properties in our schemas, so MongoDB automatically adds an _id property. The id value is filled in by the MongoDB driver <em>before</em> the document is sent to the database. And as you can see, our customer property simply holds an ObjectId, not an actual customer document. Customers are stored in the customers collection, and an instance of a customer in MongoDB looks like this:

<div>
[javascript]
{ 
	&quot;address&quot; : { 
		&quot;country&quot; : &quot;some country&quot;, 
		&quot;postalCode&quot; : &quot;1234&quot;, 
		&quot;city&quot; : &quot;some city&quot;, 
		&quot;street&quot; : &quot;some street&quot; 
	}, 
	&quot;contact&quot; : { 
		&quot;email&quot; : &quot;some.email@gmail.com&quot;, 
		&quot;name&quot; : &quot;some name&quot; 
	}, 
	&quot;includeContactOnInvoice&quot; : true, 
	&quot;_id&quot; : ObjectId(&quot;4e25f7d2041ec8c006000016&quot;), 
	&quot;vatNumber&quot; : &quot;0456.876.234&quot;, 
	&quot;name&quot; : &quot;some customer&quot;, 
	&quot;phoneNumber&quot; : &quot;123456789&quot; 
}
[/javascript]
</div>

Pretty self-explanatory i think. 

That's enough for this post. We've seen how we defined our objects in Mongoose, got a glimpse of Jasmine and covered some very basic interactions with MongoDB. I'm going to post more on this stuff as i continue working on my toy project, though i won't make any promises on how long it'll take before new posts will show up :)

This code will likely evolve significantly in the next couple of weeks/months, and if you're interested you can always follow its evolution on <a href="https://github.com/davybrion/therabbithole">github</a>.