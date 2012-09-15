This is largely common sense already, but I still frequently run into people who don't know how dangerous this is or how to properly store user credentials. The many Anonymous hacks in the past year that resulted in the leaking of users' passwords also show that many sites still store passwords in either clear-text or encrypted form. It's actually quite simple to store credentials safely, so here's a quick recap and example.

The biggest issue with storing passwords is that you have to assume that it's always possible that someone can get access to your database. Yes, even if it's not directly exposed to the outside world, which it **never** should be. Whatever security measures you've put in place to protect your database, it's a good idea to assume that sooner or later, someone will be able to punch a hole through your security measures and be able to read the data. So obviously, you really don't want to store clear-text passwords. You also don't want to store encrypted passwords because encrypted data can always be decrypted. And if people get access to those encrypted passwords even if they weren't supposed to, it'd be wise to assume that they also know how to decrypt them, or that it won't take them long to figure it out.

A much better approach is to store a hashed representation of the password instead, using a strong one-way cryptographic algorithm and a **unique** salt value per password. If the cryptographic algorithm is one-way, it means you can't apply another algorithm to get the original source value again. The only way to compare passwords is to apply the cryptographic algorithm on a given password using the originally used salt value, and then compare the resulting hash with the one you've stored. If they are identical, the given password is the same as the one that was used originally. If they differ, the password is invalid.

Attackers can still employ [rainbow tables](http://en.wikipedia.org/wiki/Rainbow_table) to try to find password values that generate the same hashes as the ones in your database. Luckily, generating rainbow tables takes time and plenty of space as well so it makes it much harder for attackers to find the passwords. This is why it's so important to use a unique salt value per password. It effectively means that a rainbow table would have to be generated for every single salt value that you've used, making it practically infeasible to find the original password values.

Let's demonstrate this with a simple example. The example is from a Node.js application, but this technique can be applied with whatever technology stack you're using. 

This is my User model:

<script src="https://gist.github.com/3728895.js?file=s1.js"></script>

Notice that the salt property of my User type has its default value set to 'uuid.v1'. In this case, uuid.v1 is a function which will be invoked by Mongoose whenever a new User instance is created. Every User instance will thus have a UUID value stored in its salt property. You can also see that I'm not storing the given passwordString in the setPassword function, but that I calculate the hash value based on the passwordString and the UUID salt value.

NOTE: the code above uses SHA-256 to create the hash. These days, a better alternative is to use bcrypt, which is specifically designed to be slow so that it makes brute forcing a much more expensive and impractical operation.

Suppose I create a user with the following code:

<script src="https://gist.github.com/3728895.js?file=s2.js"></script>

Its database representation will look like this:

<script src="https://gist.github.com/3728895.js?file=s3.js"></script>

If an attacker would get access to this, he'd have to generate a rainbow table using the salt value, which takes time, and even then he has no guarantee that the rainbow table will actually contain the correct password. Again, this is why it's so important to use a unique salt for every password. Also, you can use whatever value you want as the salt value so if you can determine it based on some other fields or by using a specific formula you don't need to store the actual salt value. It's recommended to use a long salt value though. Theoretically speaking, it's safer if the salt value isn't stored so clearly as I'm doing here, but even with the salt value clearly visible to a possible attacker, it would still be practically infeasible for him to generate all those rainbow tables.

And of course, my actual authentication function is still very simple as well:

<script src="https://gist.github.com/3728895.js?file=s4.js"></script>

So as you can see, there's nothing hard or complicated about storing credentials in a secure manner. It's quite easy to do so and there are no downsides to doing this.