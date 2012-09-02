NOTE: the examples in this post use JavaScript and Jasmine, but the advice is applicable to whatever BDD framework you use.

I'm using <a href="http://pivotal.github.com/jasmine/">Jasmine</a> for the automated tests in my breakable toy project. It's a BDD framework, very similar to RSpec so it makes it really easy to write your tests in the given-when-then style which i'm (finally) starting to like more and more. What makes given-when-then testing interesting is that you have 3 explicit steps in a test: setting up the 'given' part, doing something in the 'when' part, and asserting on it in the 'then' part. It makes it especially easy to reuse the 'givens' and even the 'whens' if you just want to add a few 'thens'.

Let's first start off with a bad example, one that i wrote about 2 weeks ago:

<div>
[javascript]
describe('given an existing customer', function() {

	describe('when it is retrieved from the database', function() {

		it('should contain the same values that have been inserted', function() {
			var customer = new CustomerBuilder()
				.withIncludeContactOnInvoice()
				.build();

			customer.save(function(err) {
				Customer.findById(customer.id, function(err, result) {
					helper.customersShouldBeEqual(result, customer);					
					asyncSpecDone();
				});
			});
			asyncSpecWait();
		});

	});

});
[/javascript]  
</div>

It's organized in a given-when-then style, but in a bad way. The only benefit that i'm getting from it here, is that the structure is sort of easy to read: given an existing customer, when it is retrieved from the database, it should contain the same values that have been inserted. When i wrote it, i knew that saving the customer should be done in the 'given' step, retrieving it should be done in the 'when' step and comparing the fields of the inserted customer and the retrieved customer should be done in the 'then' step. In this case, everything is being done in the 'then' step. 

When i wrote that code, i figured it would just be easier to do it this way, because on Node.JS every I/O call is asynchronous and i thought it would hurt readability if i were to split everything up according to the given-when-then rules due to the asynchronous calls.  But then i wanted to add tests for updating and deleting a customer. In both cases, the 'given' part would again be 'given an existing customer'. So i wanted to add them in the right place, which meant i had to choose between duplicating the code to save the customer in each test, or bite the bullet and split it up properly and deal with the asynchronous calls.

Let's start with our original example, and move the saving of the customer to the 'given' step, and the retrieval to the 'when' step:

<div>
[javascript]
describe('given an existing customer', function() {

	var customer = null;

	beforeEach(function(err) {
		customer = new CustomerBuilder()
			.withIncludeContactOnInvoice()
			.build();

		customer.save(function(err) {
			expect(err).toBeNull();
			asyncSpecDone();
		});
		asyncSpecWait();
	});

	describe('when it is retrieved from the database', function() {

		var retrievedCustomer = null;

		beforeEach(function() {
			Customer.findById(customer.id, function(err, result) {
				expect(err).toBeNull();
				retrievedCustomer = result;
				asyncSpecDone();
			});
			asyncSpecWait();
		});

		it('should contain the same values that have been inserted', function() {
			helper.customersShouldBeEqual(retrievedCustomer, customer);
		});

	});

});

[/javascript]
</div>

Despite ending up with more lines of code, there are some notable improvements here. We take advantage of the beforeEach method, which is executed once before each spec (in the case of Jasmine, a call to the 'it' method is a spec) is executed and once before each spec in each nested suite (in the case of Jasmine, a call to the 'describe' method creates a new suite) is executed. Most BDD-frameworks have something similar. Obviously, due to the asynchronous nature of Node.JS we use the asyncSpecWait() and asyncSpecDone() calls (added to Jasmine by jasmine-node) to wait until the asynchronous calls have completed before we move to the next step. In production Node.JS code, you really don't want to do this since that completely takes away the benefits of the platform, but for automated tests, it makes sense to do so. This enables us to put the right code in the right place: saving the customer is done in the 'given' step, retrieving it is done in the 'when' step, and the 'then' step only contains the code to verify that both instances are equal. If we need to verify something else about retrieved customers, we could easily add more specs (calls to the 'it' method) without having to repeat any of the setup work.

Now we can also add the tests for the update and delete scenario, within the context of the 'given an existing customer' scenario.

<div>
[javascript]
describe('given an existing customer', function() {

	var customer = null;

	beforeEach(function(err) {
		customer = new CustomerBuilder()
			.withIncludeContactOnInvoice()
			.build();
			
		customer.save(function(err) {
			expect(err).toBeNull();
			asyncSpecDone();
		});
		asyncSpecWait();
	});

	describe('when it is retrieved from the database', function() {

		var retrievedCustomer = null;

		beforeEach(function() {
			Customer.findById(customer.id, function(err, result) {
				expect(err).toBeNull();
				retrievedCustomer = result;
				asyncSpecDone();
			});
			asyncSpecWait();
		});
	
		it('should contain the same values that have been inserted', function() {
			helper.customersShouldBeEqual(retrievedCustomer, customer);
		});
		
	});
	
	describe('when it is modified and updated', function() {
			
		beforeEach(function() {	
			customer.name = 'some other customer';
			customer.vatNumber = '0456.876.235';
			customer.address = {
				street: 'some other street',
				postalCode: '12345',
				city: 'some other city'
			};
			customer.phoneNumber = '123456789';
			customer.contact = {
				name: 'some name',
				email: 'some_email@hotmail.com'
			};
			customer.save(function(err) {
				expect(err).toBeNull();
				asyncSpecDone();
			});
			asyncSpecWait();
		});

		it('contains the updated values in the database', function() {
			Customer.findById(customer.id, function(err, result) {
				helper.customersShouldBeEqual(result, customer);
				asyncSpecDone();
			});
			asyncSpecWait();
		});

	});

	describe('when it is deleted', function() {
		
		beforeEach(function() {
			customer.remove(function(err) {
				expect(err).toBeNull();
				asyncSpecDone();
			});
			asyncSpecWait();
		});		

		it('can no longer be retrieved', function() {
			Customer.findById(customer.id, function(err, result) {
				expect(result).toBeNull();
				asyncSpecDone();
			});
			asyncSpecWait();
		});

	});
	
});
[/javascript]
</div>

In this case, the only duplication we have are the calls to asyncSpecWait and asyncSpecDone, which can't really be avoided with this style of testing on the Node.JS platform. Other than that, each part of the code is focused solely on that what it needs to do. If you're using a BDD framework, be sure you leverage it to make sure each part of your testcode is as focused on its task as it can be.