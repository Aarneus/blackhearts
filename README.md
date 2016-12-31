# 72 Cases of the Blackhearts Detective Agency
NaNoGenMo 2016 Novel Generator

Better documentation may be incoming.

Sample Grammar syntax:
<blockquote>
# Random furniture<br>
furniture L<br>
=> "the [=>floor]"<br>
=> "a windowsill", plot.inside != null<br>
=> "a desk", L.office != null<br>
=> "a filing cabinet", L.office != null<br>
</blockquote>

Sample output:
<blockquote>
Chapter 1<br>

The Blackhearts Detective Agency. There's no place like home. I feel a pang of hunger in my stomach. I'd fill my fridge if I could. Problem is, a detective's pay is not exactly luxurious. I need a job. And I need it fast. I'm throwing darts at the lord barrister's picture when the phone rings. It's Nellie. She is in a hurry. I furrow my brow. It doesn't seem good. She will owe me after this. I get ready to leave for the power plant.
</blockquote>

<bold>Notes about the Grammar language</bold><br>
Syntax<br>
The most basic rule is this simple one:<br>
<code>color => "red"</code><br>
This is just a basic rule like those found in all context-free grammars.<br>
If you want to invoke other rules (i.e. nonterminal symbols) you do it like this:<br>
<code>key => "The [=>color] key."</code><br>
If you want to employ variables, the statements go after the text:<br>
<code>next_room => "kitchen", current_room == "foyer"</code><br>
You can use flags to allow a similar rule to only apply once (e.g. so no two rules describing weather are used)<br>
<code> => "It was raining.", flag weather</code><br>
Rules can have parameters, where you can pass a variable to a rule:<br>
<code>name E => "she", previous_name == E.name<br>
name E => "E.name", previous_name != E.name, previous_name = E.name</code><br>

State model<br>
The variables are divided into local and global variables.<br>
Local variables are those that start with a capital letter.<br>
Variables are all rootnodes and can have subvariables that are accessed by dot notation:<br>
<code>dog.owner.opinion.(dog.name)</code><br>
Variables can be set by either modifying their value or by replacing them with another variable:<br>
<code>let dog.name = "Spot"<br>
merchant.pet <- dog</code><br>
In the previous example, merchant.pet.name would now be "Spot".<br>
If the variable or subvariable does not exist, it is equal to null:<br>
<code>cat.wings == null</code><br>
Replacing a variable with null will delete that subvariable (and all of its subvariables, etc):<br>
<code>cat.wings <- null</code><br>

Rule selection<br>
The statements after the text on a rule are sorted into two categories; conditions and effects.
Conditions are any statement that has one of the conditional operators (==, != <, >, >=, <=) and determine if the rule is possible at the given time.<br>
Effects are what happens after the rule has been selected.<br>
If multiple rules are available for a non-terminal, the applied rule is selected randomly, with each rule weighted by the number of conditions it has, so the rule with the most conditions has the highest chance of being chosen.<br>
This heuristic has been appropriated from Evans's presentation on Valve's dialogue system.<br>
If no rule applies, an empty string ("") is used instead.<br>
