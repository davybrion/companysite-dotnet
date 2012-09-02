Note: if you haven't read my introductory post about Mongoose, you might want to do so <a href="http://davybrion.com/blog/2011/07/first-steps-with-mongodb-mongoose-and-jasmine-node-on-node-js/" target="_blank">first</a> if you haven't seen anything about it yet.

I wanted to add an Invoice entity to my breakable toy project, and came up with this mongoose Schema:

<div>
[javascript]
var invoiceSchema = new Schema({
	companyId: { type: ObjectId, required: true },
	customerId: { type: ObjectId, required: true },
	invoiceNumber: { type: String, required: true, unique: true },
	date: { type: Date, required: true },
	dueDate: { type: Date, required: true }, 
	paid: { type: Boolean, required: true, default: false },
	activityId: { type: ObjectId, required: true },
	totalHours: { type: Number, required: true },
	hourlyRate: { type: Number, required: true },
	totalExcludingVat: { type: Number, required: true },
	vat: { type: Number, required: true }, 
	totalIncludingVat: { type: Number, required: true } 
});
[/javascript]
</div>

Notice the totalExcludingVat, vat and totalIncludingVat properties. For the kind of work i do, the VAT percentage will always be the same. So i wanted the vat and totalIncludingVat properties to just be calculated automatically whenever the totalExcludingVat property was set. Mongoose makes it possible to define <a href="http://mongoosejs.com/docs/getters-setters.html" target="_blank">custom getters and setters</a> but according to the documentation, the purpose of a setter is to transform the value being set into something different for the underlying document. In my case, i want the value being set to remain the same, but i want the vat and totalIncludingVat properties to just be calculated on the spot. Despite it probably not being the use case that was originally envisioned for the custom setters, i added this to the Invoice type:

<div>
[javascript]
invoiceSchema.path('totalExcludingVat').set(function(value) {
	this.vat = value * 0.21;
	this.totalIncludingVat = value * 1.21;
	return value;
});
[/javascript]
</div>

And this actually works pretty nicely:

<div>
[javascript]
describe('given an invoice', function() {

	var invoice = new Invoice();

	describe('when you set its totalExcludingVat property', function() {
		
		invoice.totalExcludingVat = 1000;

		it('should automatically set the vat property', function() {
			expect(invoice.vat).toEqual(210);
		});

		it('should automatically set the totalIncludingVat property', function() {
			expect(invoice.totalIncludingVat).toEqual(1210);
		});

	});
});

describe('given an invoice created with a totalExcludingVat value', function() {

	var invoice = new Invoice({ totalExcludingVat: 1000 });

	it('should contain the correct vat value', function() {
		expect(invoice.vat).toEqual(210);
	});

	it('should contain the correct totalIncludingVat property', function() {
		expect(invoice.totalIncludingVat).toEqual(1210);
	});
});
[/javascript]
</div>

What i like about this approach is that even though i defined a 'setter', i didn't have to define a useless getter like i would have to do in C#. I'm also curious how Mongoose implements the setter behavior. Take a look at this line:

<div>
[javascript]
invoice.totalExcludingVat = 1000;
[/javascript]
</div>

In C#, that would call a compiler generated set <em>method</em>. In JavaScript, no function is executed when something like this is done. It's essentially exactly the same as doing this:

<div>
[javascript]
invoice['totalExcludingVat'] = 1000;
[/javascript]
</div>

Not sure how they got that working, but i'm certainly glad that they did.

And yes, i'll eventually look into how they actually made it work but it's getting late and i need to get up for work in about 6 hours so i think i'll skip on getting into those details for now ;)