using System;
using System.IO;
using System.Linq;
using DSI.Deck;

namespace DeckSample
{
    class Program
    {
        /// <summary>
        /// This is an example usage of the Deck class library.
        /// </summary>
        static void Main() {

            //Creates a new deck (default size is 52 cards), prints out all cards
            var deck = new Deck();
            Console.WriteLine("Cards in deck prior to shuffle:");
            var i = 1;
            foreach (var card in deck) {
                Console.Write($"{card.FullName,20}");
                if (i++ % 5 == 0) Console.Write("\n");
            }
            

            //Shuffles the deck and prints out the cards again.
            Console.WriteLine("\n\nCards in deck after shuffle:");
            deck.Shuffle();
            i = 1;
            foreach (var card in deck) {
                Console.Write($"{card.FullName,20}");
                if (i++ % 5 == 0) Console.Write("\n");
            }

            //Cuts the deck (at 26) and prints out the cards again.
            Console.WriteLine("\n\nCards in deck after cutting (halfway):");
            deck.Cut(26);
            i = 1;
            foreach (var card in deck) {
                Console.Write($"{card.FullName,20}");
                if (i++ % 5 == 0) Console.Write("\n");
            }

            //Draws 5 non-sorted cards and prints them out.
            Console.WriteLine("\n\nDrawing 5 cards:");
            var drawn = deck.Draw(5);
            i = 1;
            foreach (var card in drawn) {
                Console.Write($"{card.FullName,20}");
                if (i++ % 5 == 0) Console.Write("\n");
            }

            //Draws 5 sorted cards and prints them out.
            Console.WriteLine("\nDrawing 5 sorted cards:");
            var sortedHand = deck.DrawSorted(5);
            i = 1;
            foreach (var card in sortedHand) {
                Console.Write($"{card.FullName,20}");
                if (i++ % 5 == 0) Console.Write("\n");
            }

            Console.WriteLine($"\nCurrent number of drawn cards: {deck.DrawnCount}");
            Console.WriteLine($"Current number of cards in deck: {deck.CurrentCount}");
            Console.WriteLine($"Total number of cards in deck: {deck.TotalCount}\n");

            var cardCount = drawn.Count;
            deck.Push(drawn);
            Console.WriteLine($"Cards in deck after returning non-sorted hand of {cardCount} cards: {deck.CurrentCount}\n");

            cardCount = sortedHand.Count;
            deck.Push(sortedHand);
            Console.WriteLine($"Cards in deck after returning sorted hand of {cardCount} cards: {deck.CurrentCount}\n");
            Console.WriteLine($"After returning cards, there are {drawn.Count} cards in the non-sorted hand and {sortedHand.Count} cards in the sorted hand.\n");
            Console.WriteLine($"There are now {deck.CurrentCount} cards in the deck.");

            var singleCard = deck.DrawOne();
            Console.WriteLine($"\nDrew a single card: {singleCard.FullName}. {deck.CurrentCount} cards left in the deck.");

            deck.Push(singleCard);
            Console.WriteLine("Returned card to deck.");

            Console.WriteLine("\nDrawing a card to insert into the deck");
            var cardToInsert = deck.DrawOne();
            Console.WriteLine($"We drew a {cardToInsert.FullName}! Inserting it into the 5th index of the deck");
            Console.WriteLine($"Before the insert, the 5th index (6th card) is a {deck.ElementAt(5).FullName}");
            deck.Insert(cardToInsert,5);
            Console.WriteLine($"After the insert, it is a {deck.ElementAt(5).FullName}");

            Console.WriteLine("\nDrawing the Ace of Spades from the deck by name...");
            var namedCard = deck.DrawOne("Ace of Spades");
            Console.WriteLine($"We drew an {namedCard.FullName}! Returning it to the top of the deck.");
            deck.Push(namedCard);


            //Create a hand of 5 cards.
            var hand = deck.Draw(5);
            Console.WriteLine("\nDrawing a hand of 5 cards: ");
            foreach (var card in hand) {
                Console.WriteLine($"{card.FullName,18}");
            }

            //Return the first card to the deck. Removes the card from the hand.
            Console.WriteLine("\nReturning the first card back to the deck.");
            deck.Push(hand.First(), hand);

            Console.WriteLine("Our hand after returning the first card: ");
            foreach (var card in hand) {
                Console.WriteLine($"{card.FullName,18}");
            }

            //Returns the rest of the cards in the hand to the deck.
            Console.WriteLine("\nReturning the remaining cards to the deck.");
            deck.Push(hand);
            Console.WriteLine("Our hand after returning the remaining cards (should be empty): ");
            foreach (var card in hand) {
                Console.WriteLine($"{card.FullName,18}");
            }

            //Take a peek at the top card in the deck without drawing it.
            Console.WriteLine($"Peeking at the top card... It's a {deck.Peek().FullName}!");
            
            Console.ReadKey();
        }
    }
}
