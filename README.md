# mustache#

An extension of the mustache text template engine for .NET.

Download using NuGet: [mustache#](http://nuget.org/packages/mustache-sharp)

## Overview
Generating text has always been a chore. Either you're concatenating strings like a mad man or you're getting fancy with `StringBuilder`. Either way, the logic for conditionally including values or looping over a collection really obscures the intention of the code. A more declarative approach would improve your code big time. Hey, that's why server-side scripting got popular in the first place, right?

[mustache](http://mustache.github.com/) is a really simple tool for generating text. .NET developers already had access to `String.Format` to accomplish pretty much the same thing. The only problem was that `String.Format` used indexes for placeholders: `Hello, {0}!!!`. **mustache** let you use meaningful names for placeholders: `Hello, {{name}}!!!`.

**mustache** is a logic-less text generator. However, almost every time I've ever needed to generate text I needed to turn some of it on or off depending on a value. Not having the ability to turn things off usually meant going back to building my text in parts. 

Introducing [handlebars.js](http://handlebarsjs.com/)... If you've needed to generate any HTML templates, **handlebars.js** is a really awesome tool. Not only does it support an `if` and `each` tag, it lets you define your own tags! It also makes it easy to reference nested values `{{Customer.Address.ZipCode}}`.

**mustache#** brings the power of **handlebars.js** to .NET and then takes it a little bit further. Not only does it support the same tags, it also handles whitespace intelligently. **mustache#** will automatically remove lines that contain nothing but whitespace and tags. This allows you to make text templates that are easy to read.

    Hello, {{Customer.Name}}
    
    {{#with Order}}
    {{#if LineItems}}
    Here is a summary of your previous order:
    
    {{#each LineItems}}
        {{ProductName}}: {{UnitPrice:C}} x {{Quantity}}
    {{/each}}
    
    Your total was {{Total:C}}.
    {{#else}}
    You do not have any recent purchases.
    {{/if}}
    {{/with}}
    
Most of the lines in the previous example will never appear in the final output. This allows you to use **mustache#** to write templates for normal text, not just HTML/XML.

## Placeholders
The placeholders can be any valid identifier. These map to the property names in your classes.

### Formatting Placeholders
Each format item takes the following form and consists of the following components:

    {{identifier[,alignment][:formatString]}}

The matching braces are required. Notice that they are double curly braces! The alignment and the format strings are optional and match the syntax accepted by `String.Format`. Refer to [String.Format](http://msdn.microsoft.com/en-us/library/system.string.format.aspx)'s documentation to learn more about the standard and custom format strings.

### Placeholder Scope
The indentifier is used to find a property with a matching name. If you want to print out the object itself, you can use the special identifier `this`.

    FormatCompiler compiler = new FormatCompiler();
    Generator generator = compiler.Compiler("Hello, {{this}}!!!");
    string result = generator.Render("Bob");
    Console.Out.WriteLine(result);  // Hello, Bob!!!
    
Some tags, such as `each` and `with`, change which object the values will be retrieved from.

If a property with the placeholder name can't be found at the current scope, the name will be searched for at the next highest level.

**mustache#** will automatically detect when an object is a dictionary and search for matching key. In this case, it still needs to be a valid identifier name.

### Nested Placeholders
If you want to grab a nested property, you can separate identifiers using `.`.

    {{Customer.Address.ZipCode}}

## The 'if' tag
The **if** tag allows you to conditionally include a block of text.

    Hello{{#if Name}}, {{Name}}{{/if}}!!!

The block will be printed if:
* The value is a non-empty string.
* The value is a non-empty collection.
* The value is a char and isn't the NULL char.
* The value is a non-zero number.
* The value evaluates to true.

The **if** tag has complimentary **elif** and **else** tags. There can be as many **elif** tags as desired but the **else** tag must appear only once and after all other tags.

    {{#if Male}}Mr.{{#elif Married}}Mrs.{{#else}}Ms.{{/if}}

## The 'each' tag
If you need to print out a block of text for each item in a collection, use the **each** tag.

    {{#each Customers}}
    Hello, {{Name}}!!
    {{/each}}
    
Within the context of the **each** block, the scope changes to the current item. So, in the example above, `Name` would refer to a property in the `Customer` class.
    
## The 'with' tag
Within a block of text, you may refer to a same top-level placeholder over and over. You can cut down the amount of text by using the **with** tag.

    {{#with Customer.Address}}
    {{FirstName}} {{LastName}}
    {{Line1}}
    {{#if Line2}}{{Line2}}{{/if}}
    {{#if Line3}}{{Line3}}{{/if}}
    {{City}} {{State}}, {{ZipCode}}
    {{/with}}
    
Here, the `Customer.Address` property will be searched first for the placeholders. If a property cannot be found in the `Address` object, it will be searched for in the `Customer` object and on up.