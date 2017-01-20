using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DSI.Deck;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeckOfCardsTests
{
    [TestClass]
    public class DeckTester
    {

        /// <summary>
        /// Simple test to confirm that if we draw card(s) from the deck, the number of drawable card(s)
        /// is changed to reflect the draw.
        /// </summary>
        [TestMethod]
        public void Draw_CountTest() {
            var fullDeckSize = 52;
            var cardsToDraw = 7;
            var deck = new Deck(fullDeckSize);

            var size = deck.CurrentCount; //no cards drawn, size is full deck
            if (size != fullDeckSize) throw new ApplicationException($"Deck has incorrect number of cards: {fullDeckSize} vs {size}.");

            var drawn = deck.Draw(cardsToDraw);
            if (drawn.Count != cardsToDraw) throw new ApplicationException($"Deck has incorrect number of cards: {fullDeckSize} vs {size}."); //drew correct number of cards
            if(deck.CurrentCount != fullDeckSize - cardsToDraw) throw new ApplicationException($"Deck has incorrect number of cards: {fullDeckSize} vs {size}.");

            var newCardsToDraw = 7;
            var newDrawn = deck.Draw(newCardsToDraw);
            if (newDrawn.Count != newCardsToDraw) throw new ApplicationException($"Deck has incorrect number of cards: {fullDeckSize} vs {size}."); //drew correct number of cards
            if (deck.CurrentCount != fullDeckSize - cardsToDraw - newCardsToDraw) throw new ApplicationException($"Deck has incorrect number of cards: {fullDeckSize} vs {size}.");
        }

        [TestMethod]
        public void ReturnAllCards_CountTest(){
            var deck = new Deck();
            var hand = deck.Draw(5);
            if(deck.DrawnCount != 5 || deck.CurrentCount != 47) throw new ApplicationException($"After drawing, card counts are off.");
            deck.ReturnAllCards();
            if (deck.DrawnCount != 0 || deck.CurrentCount != 52) throw new ApplicationException($"After returning cards, card counts are off.");
            
        }
        [TestMethod]
        public void Push_CountTest(){
            var deck = new Deck();
            var hand = deck.Draw(5);
            if(deck.DrawnCount != 5 || deck.CurrentCount != 47) throw new ApplicationException($"After drawing, card counts are off.");
            deck.Push(hand);
            if (deck.DrawnCount != 0 || deck.CurrentCount != 52) throw new ApplicationException($"After returning cards, card counts are off.");
            var card = deck.DrawOne();
            if (deck.DrawnCount != 1 || deck.CurrentCount != 51) throw new ApplicationException($"After drawing cards, card counts are off.");
            deck.Push(card);
            if (deck.DrawnCount != 0 || deck.CurrentCount != 52) throw new ApplicationException($"After returning cards, card counts are off.");
            hand = deck.Draw(3);
            if (deck.DrawnCount != 3 || deck.CurrentCount != 49) throw new ApplicationException($"After drawing cards, card counts are off.");
            deck.Push(hand.First(), hand);
            if (deck.DrawnCount != 2 || deck.CurrentCount != 50) throw new ApplicationException($"After returning cards, card counts are off.");
        }
        [TestMethod]
        public void Insert_CountTest(){
            var deck = new Deck();
            
            var card = deck.DrawOne();
            if (deck.DrawnCount != 1 || deck.CurrentCount != 51) throw new ApplicationException($"After drawing cards, card counts are off.");
            deck.Insert(card,12);
            if (deck.DrawnCount != 0 || deck.CurrentCount != 52) throw new ApplicationException($"After inserting cards, counts are off.");
            var hand = deck.Draw(5);
            if (deck.DrawnCount != 5 || deck.CurrentCount != 47) throw new ApplicationException($"After drawing cards, card counts are off.");
            deck.Insert(hand,12);
            if (deck.DrawnCount != 0 || deck.CurrentCount != 52) throw new ApplicationException($"After inserting cards, counts are off.");
        }
        [TestMethod]
        public void Insert_IndexTest(){
            var deck = new Deck();
            var index = 12;
            var cardToInsert = deck.DrawOne();
            var cardAtIndex = deck.ElementAt(index);
            deck.Insert(cardToInsert,index);
            if(deck.ElementAt(index).FullName != cardToInsert.FullName) throw new ApplicationException($"Inserted card not appearing at indicated index.");
            if(deck.ElementAt(index+1).FullName != cardAtIndex.FullName) throw new ApplicationException($"Insert operation failed - previously indexed card should be after the inserted card.");
            
        }
        [TestMethod]
        public void Cut_OrderTest_CountTest(){
            var deck = new Deck();
            deck.Cut();
            if(deck.CurrentCount != 52) throw new ApplicationException($"After cutting the deck, counts are off.");
            deck = new Deck();
            var firstCard = deck.First();
            deck.Cut(0);
            if(deck.ElementAt(deck.CurrentCount-1) != firstCard) throw new ApplicationException($"After cutting the deck, the indexes are off.");
            if(deck.CurrentCount != 52) throw new ApplicationException($"After cutting the deck, counts are off.");

        }

        /// <summary>
        /// Confirm that if we draw a hand from the deck, all of those cards are are listed as drawn from the deck.
        /// </summary>
        [TestMethod]
        public void Draw_CardMarkedAsDrawn() {
            var deck = new Deck();
            var drawn = deck.Draw(3);
            if (deck.DrawnCards.All(c => drawn.All(card => card.Equals(c))))
                throw new InvalidDataException("Drawn cards should be listed as such.");
            if (deck.Any(remainingCard => drawn.Any(card => card.Equals(remainingCard))))
                throw new InvalidDataException("Drawn cards should be listed as such.");
        }

        /// <summary>
        /// Tests all deck constructors to confirm that the amount of cards in the deck is accurate.
        /// </summary>
        [TestMethod]
        public void CreateDeck_CountTest() {
            var sizeOfDeck = 52;
            var deck = new Deck(sizeOfDeck);
            if (deck.CurrentCount + deck.DrawnCount != sizeOfDeck) throw new ApplicationException($"Deck should have {sizeOfDeck} cards, but has {deck.CurrentCount + deck.DrawnCount}.");
            deck = new Deck(30);
            if (deck.CurrentCount != 30) throw new ApplicationException($"After creating, counts are off.");

            deck = new Deck(80, true);
            if (deck.CurrentCount != 80) throw new ApplicationException($"After creating, counts are off.");

            deck = new Deck(80);
            if (deck.CurrentCount != 80) throw new ApplicationException($"After creating, counts are off.");

            var copyDeck = new Deck(deck);
            if (copyDeck.CurrentCount != deck.CurrentCount || copyDeck.DrawnCount != deck.DrawnCount)
                throw new ApplicationException($"After creating, counts are off.");

        }

        /// <summary>
        /// Create a deck (unshuffled) and create a copy of it. Shuffle the original deck
        /// and confirm that the copy and original don't have identical sequences.
        /// Then creates a newly shuffled deck and confirms that the two shuffled decks don't
        /// have identical sequences.
        /// </summary>
        [TestMethod]
        public void Shuffle_IsDifferentOrder() {
            var deck = new Deck();

            var original = new Deck(deck);
            deck.Shuffle();
            if(original.SequenceEqual(deck)) throw new ApplicationException("Shuffled deck is same as non-shuffled.");

            var shuffled = new Deck(deck);
            deck.Shuffle();
            if(shuffled.SequenceEqual(deck)) throw new ApplicationException("After shuffling, no change.");
        }

        /// <summary>
        /// Creates a shuffled deck, draws a sorted hand of 13 cards, and confirms that 
        /// the hand is in-fact sorted.
        /// </summary>
        [TestMethod]
        public void DrawSorted_IsSorted() {
            var deck = new Deck();
            deck.Shuffle();
            var sorted = deck.DrawSorted(13);
            for (var i = 1; i < sorted.Count; i++) {
                var first = sorted[i - 1];
                var second = sorted[i];

                //higher rank suit should never appear first.
                if(first.Suit > second.Suit) throw new ApplicationException($"Sorted cards are out of order...");

                //higher value should only be the first card if the second card's 
                //suit is a higher than the first.
                if(first.Value > second.Value && second.Suit <= first.Suit) throw new ApplicationException($"Sorted cards are out of order...");

                
            }
        }

        /// <summary>
        /// This test confirms that if we draw a list of cards, the deck is reduced by the amount of the list.
        /// After drawing the cards, we ensure the cards can no longer be drawn from the deck.
        /// We then return the cards to the deck, ensuring the deck is increased by that same number of cards.
        /// Then we check and confirm that the card we drew is now back in the deck.
        /// Since this is a list of cards, we also confirm that the list we drew (our hand) is empty now that the cards
        /// are back in the deck.
        /// </summary>
        [TestMethod]
        public void DrawCards_ReturnCards_DuplicateCheck() {
            var deck = new Deck(52);
            var hand = deck.Draw(5);
            var nameList = new List<string>();

            foreach (var card in hand) {
                var name = card.FullName;
                nameList.Add(name);
                //if there exists another drawable card with the same name as the card we drew, error!
                if (deck.Any(p => p.FullName == name)) throw new ApplicationException($"Duplicate card: ({name}) found!");

                //if the card we drew is not listed as 'drawn' from the deck, error!
                if (deck.DrawnCards.All(p => p.FullName != name)) throw new ApplicationException($"Drawn card ({name}) not marked as removed from deck...");

                //if the deck has not reduced by one card after drawing one card, error!
                if (deck.CurrentCount != deck.TotalCount-hand.Count) throw new ApplicationException($"We drew a {name} but the drawable cards didn't decrease by one.");

            }
            var handSize = hand.Count;
            deck.Push(hand);

            //our hand should be empty now.
            if(hand.Count != 0) throw new ApplicationException($"After returning {handSize} cards to deck of {handSize} cards, hand still has {hand.Count} cards in it.");

            
            //all cards drawn and returned should now be available.
            foreach (var cardName in nameList) {
                if(deck.All(p => p.FullName != cardName)) throw new ApplicationException($"{cardName} should be in the list of drawable cards but isn't.");
                if(deck.DrawnCards.Any(p=>p.FullName != cardName)) throw new ApplicationException($"{cardName} should be drawable, but is listed as drawn from the deck.");
            }

        }

        /// <summary>
        /// This is a simple test that confirms that if we draw a hand of 5 cards, then return one, our hand contains
        /// 4 cards, and the card we just returned is now available to be drawn from the deck.
        /// </summary>
        [TestMethod]
        public void Draw_DrawHand_ReturnPart_CountTest() {
            var deck = new Deck(52);
            var hand = deck.Draw(5);

            var firstCardInHand = hand.First();
            deck.Push(firstCardInHand, hand);

            //Our hand should no longer contain the card we returned to the deck.
            if(hand.Contains(firstCardInHand))
                throw new ApplicationException($"We returned a {firstCardInHand.FullName} but it is still in a hand.");

            //Our returned card should be marked as drawable after returning to deck/
            if (deck.DrawnCards.Any(p=>p.FullName == firstCardInHand.FullName))
                throw new ApplicationException($"We returned a {firstCardInHand.FullName} but it is listed as drawn.");

            //Our returned card should not be marked as drawn after returning to the deck.
            if(deck.All(p=>p.FullName == firstCardInHand.FullName))
                throw new ApplicationException($"We returned a {firstCardInHand.FullName} but it is not listed as drawable.");

        }

        /// <summary>
        /// Confirms card is not drawable and not in deck after being drawn.
        /// </summary>
        [TestMethod]
        public void DrawOne_RemovesFromDeck_CardMarkedAsDrawn() {
            var deck = new Deck(52);
            var card = deck.DrawOne();
            
            if(card.CurrentlyInDeck) throw new ApplicationException($"Drawn card {card.FullName} thinks it is still in the deck.");
            if(deck.Contains(card)) throw new ApplicationException($"Drawn card {card.FullName} still exists in the deck.");

        }

        /// <summary>
        /// Created a deck and checks to ensure that no two cards have the same name... 
        /// This is expected behaviour for decks of less than 53 cards.
        /// </summary>
        [TestMethod]
        public void CreateDeck_StandardOrLessSize_DuplicateCheck() {
            //Checking a deck less than standard size
            var deck = new Deck(30);
            var duplicates = deck.GroupBy(c => c.FullName).Any(d => d.Count() > 1);
            if(duplicates) throw new ApplicationException("Duplicate card found");
            
            //Checking standard size deck
            deck = new Deck();
            duplicates = deck.GroupBy(c => c.FullName).Any(d => d.Count() > 1);
            if (duplicates) throw new ApplicationException("Duplicate card found");
        }

        /// <summary>
        /// Tests deck creation larger than 52 cards.
        /// 
        /// </summary>
        [TestMethod]
        public void CreateDeck_GreaterthanStandardSize_DuplicateCheck()
        {
            //There should be duplicates - exactly 1 of each card.
            var deck = new Deck(104);
            var exactlyTwo = deck.GroupBy(c => c.FullName).Any(d => d.Count() != 2);
            if (exactlyTwo) throw new ApplicationException("Duplicate card found");
            
        }

        /// <summary>
        /// As shuffling modifies the list, if it is threadsafe, we should get an identical card count while
        /// shuffling in one thread and checking its length in another.
        /// </summary>
        [TestMethod]
        public void ThreadTest_Shuffle_CardCount()
        {
            var deck = new Deck();
            
            var shuffler = new Thread(() => ShuffleThread(deck));
            var sizer = new Thread(() => CheckDeckLength(deck));

            shuffler.Start();
            sizer.Start();
        }

        /// <summary>
        /// Checkes the length of a deck 10000 times.
        /// </summary>
        /// <param name="deckToCheck">The deck to check.</param>
        [TestMethod]
        public void CheckDeckLength(Deck deckToCheck) {
            var startSize = deckToCheck.CurrentCount;
            if (Enumerable.Range(0, 10000).Any(i => deckToCheck.CurrentCount != startSize)) {
                throw new ThreadStateException("$Thread access violation... count is off.");
            }
        }

        /// <summary>
        /// Shuffles a deck 10000 times.
        /// </summary>
        /// <param name="deckToShuffle">The deck to shuffle.</param>
        [TestMethod]
        public void ShuffleThread(Deck deckToShuffle) {
            Enumerable.Range(0,10000).ToList().ForEach(p => deckToShuffle.Shuffle());
        }

        /// <summary>
        /// Attempts to draw more cards than are available to be drawn from the deck. 
        /// Now throws an IndexOutOfRangeException to avoid confusion later with mismatching hand sizes.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Draw_MoreCardsThanInDeck() {
            var deckSize = 52;
            var cardsToDraw = 53;
            var deck = new Deck(deckSize);

            var hand = deck.Draw(cardsToDraw);
            deck.Push(hand);
        }



        /// <summary>
        /// Should throw an exception if we try to draw a negative number of cards.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Draw_NegativeCards() {
            var deck = new Deck();
            var error = deck.Draw(-1);
            deck.Push(error);
            
        }
        /// <summary>
        /// Should throw an exception if we try add a null card to the deck..
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Push_NullCheck() {
            var deck = new Deck();
            var errorCard = deck.DrawOne();
            var errorHand = deck.Draw(3);
            errorHand = null;
            errorCard = null;
            deck.Push(errorCard);
            deck.Push(errorHand);
        }
        
        /// <summary>
        /// Should throw an exception if we try to create a deck with a negative number of cards.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDeck_NegativeCards() {
            var error = new Deck(-1);
            error.Shuffle();
            
        }
    }
}
