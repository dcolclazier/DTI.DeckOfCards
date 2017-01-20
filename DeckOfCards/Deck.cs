using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DSI.Deck
{
    
    /// <summary>
    /// Represents a deck of cards.
    /// </summary>
    public class Deck : IEnumerable<Card>
    {
        /// <summary>
        /// Shuffle the deck. By default, all drawn cards are returned to deck.
        /// </summary>
        /// <param name="onlyRemaining">Only shuffle the remaining cards in the deck.</param>
        public void Shuffle(bool onlyRemaining = false) {

            //copy our stack of cards to a list so we can randomize it.
            if (!onlyRemaining) ReturnAllCards();

            lock (Locker) {
                //Randomize our list
                var shuffledDeck = _masterDeck.OrderBy(p => Rng.Next()).ToList();
                //Clear our master list so we don't end up with any duplicates
                _masterDeck.Clear();
                //Push our shuffled list to the deck.
                Push(shuffledDeck);
            }
        }

        /// <summary>
        /// Returns all cards currently not in the deck to the deck... Warning - this has no knowledge of lists the cards
        /// currently reside in. If you want the 'hand' the card was in to reflect adding the card(s) back to the deck, use 
        /// Push(Card,List) or Push(List) instead.
        /// </summary>
        public void ReturnAllCards() {
            lock (Locker) {
                foreach (var card in _drawnCards) {
                    _masterDeck.Push(card);
                }
                _drawnCards.Clear();
            }
        }

        /// <summary>
        /// Draw a single card from the top of the deck.
        /// </summary>
        /// <returns>The card that is drawn.</returns>
        public Card DrawOne() {
            lock (Locker){
                var card = _masterDeck.Pop();
                _drawnCards.Add(card);
                card.DrawFromDeck();
                return card;
            }
        }
        
        /// <summary>
        /// Draw a single card from the deck by name.
        /// </summary>
        /// <returns>The card that is drawn.</returns>
        /// <param name="cardName">The name of the card to draw. Example: "Ace of Spades", "Two of Hearts", etc.</param>
        public Card DrawOne(string cardName) {

            var indexToDraw = _masterDeck.ToList().FindIndex(c => c.FullName == cardName);
            if (indexToDraw == -1)
                throw new ApplicationException($"Tried to draw a {cardName}, but it is not in the deck. Did you get the name right?");
            return DrawOne(indexToDraw);
        }

        /// <summary>
        /// Draw a single card from the deck by index.
        /// </summary>
        /// <returns>The card that is drawn.</returns>
        /// <param name="indexOfCard">The index of the card to draw.</param>
        public Card DrawOne(int indexOfCard) {
            if(indexOfCard < 0 || indexOfCard > _masterDeck.Count-1) throw new IndexOutOfRangeException("You tried to draw a card from the deck, but the index is out of range.");
            
            lock (Locker) {
                var cardList = _masterDeck.ToList();
                var cardToDraw = cardList.ElementAt(indexOfCard);

                cardList.Remove(cardToDraw);
                _drawnCards.Add(cardToDraw);
                ConvertListToDeck(cardList);

                cardToDraw.DrawFromDeck();
                return cardToDraw;
            }
        }

        /// <summary>
        /// DrawFromDeck a the specified number of cards from the deck.
        /// </summary>
        /// <param name="howMany">how many cards to draw.</param>
        /// <returns>the list of drawn cards</returns>
        public List<Card> Draw(int howMany) {

            if (howMany < 0 || howMany > _masterDeck.Count)
                throw new IndexOutOfRangeException($"Cannot draw a negative number of cards, or maybe you're trying to draw too many ({howMany})?");

            return Enumerable.Range(0, howMany).Select(i => DrawOne()).ToList();
        }

        /// <summary>
        /// DrawFromDeck the specified number of cards from the deck and return them in sorted order (first by suit, then by face value).
        /// </summary>
        /// <param name="howMany">how many cards to draw.</param>
        /// <returns>a list of drawn cards, sorted by suit and value (rank)</returns>
        public List<Card> DrawSorted(int howMany) {
            if (howMany < 0 || howMany > _masterDeck.Count)
                throw new IndexOutOfRangeException("Cannot draw a negative number of cards, or maybe you're trying to draw too many?");

            return Draw(howMany).OrderBy(l => l.Suit).ThenBy(g => g.Value).ToList(); 
        }

        /// <summary>
        /// Returns a single card to the deck. 
        /// </summary>
        /// <param name="card">The card to return to the deck.</param>
        public void Push(Card card) {
            if(card == null) throw new ArgumentNullException(nameof(card), "The card is null...");
            
            card.ReturnToDeck();
            lock (Locker) {
                _drawnCards.Remove(card);
                _masterDeck.Push(card);
            }
        }

        /// <summary>
        /// Returns a single card to the deck, removing it from the list (hand) it previously resided in. 
        /// </summary>
        /// <param name="card">The card to return to the deck.</param>
        /// <param name="originHand">The list ('hand') the card belonged to prior to re-entering the deck. Providing this causes card
        /// to be removed from this list! </param>
        public void Push(Card card, List<Card> originHand) {
            if(card == null || originHand == null) throw new ArgumentNullException(nameof(card), $"The {card} or hand is null...");
            if(card.CurrentlyInDeck) throw new ApplicationException($"Trying to push a {card} already in the deck.");
            if(!originHand.Contains(card)) throw new ApplicationException($"The list does not contain the {card}.");

            lock (Locker) {
                originHand.Remove(card);
            }
            Push(card);
        }

        /// <summary>
        /// Returns a list of cards to the deck, removing them from the provided list.
        /// </summary>
        /// <param name="cardsToReturn">The list of cards that we're adding back to the deck.</param>
        public void Push(List<Card> cardsToReturn) {
            if(cardsToReturn == null) throw new ArgumentNullException(nameof(cardsToReturn), "The list of cards is null...");

            //push cards in reverse order, to ensure the first card in the list is the top of the deck.
            for (var i = cardsToReturn.Count-1; i >= 0; i--) {
                Push(cardsToReturn[i]);
            }
            lock (Locker) {
                cardsToReturn.Clear();
            }
        }

        /// <summary>
        /// Inserts a card into a particular index of the deck.
        /// </summary>
        /// <param name="card">The card to insert</param>
        /// <param name="index">The index at which to insert the card. If less than 0 (or not provided) index is 
        /// chosen at random.</param>
        public void Insert(Card card, int index = -1) {
            if(card == null) throw new ArgumentNullException(nameof(card), "Cannot insert a null card.");

            if (index < 0) index = Rng.Next(1, _masterDeck.Count - 1);

            var before = _masterDeck.Take(index);
            var after = _masterDeck.Skip(index).Take(_masterDeck.Count - index);

            var newList = before.ToList();
            newList.Add(card);
            newList.AddRange(after);

            _drawnCards.Remove(card);

            ConvertListToDeck(newList);
            
        }

        /// <summary>
        /// Inserts a list of cards into a particular index of the deck.
        /// </summary>
        /// <param name="cards">The card to insert</param>
        /// <param name="startIndex">The index at which to insert the card. If less than 0 (or not provided) index is 
        /// chosen at random.</param>
        public void Insert(List<Card> cards, int startIndex = -1) {
            if (cards == null) throw new ArgumentNullException(nameof(cards), "Cards to insert cannot be null.");
            if (startIndex < 0) startIndex = Rng.Next(1, _masterDeck.Count - 1);

            var before = _masterDeck.Take(startIndex);
            var after = _masterDeck.Skip(startIndex).Take(_masterDeck.Count - startIndex);

            var newList = before.ToList();
            newList.AddRange(cards);
            newList.AddRange(after);

            ConvertListToDeck(newList);
            
        }

        /// <summary>
        /// Returns the top card by default, or the card of the index in the deck without removing it.
        /// </summary>
        /// <returns>The card we want to peek at.</returns>
        /// <param name="index">The index of the card you want to peek at.</param>
        public Card Peek(int index = 0) {
            lock (Locker) {
                return _masterDeck.ElementAt(index);
            }
        }

        /// <summary>
        /// "Cuts" the deck. 
        /// Example: deck.Cut(0) takes every card after the top card and places the group on top, effectively placing 
        ///          the top card on the bottom of the deck.
        /// </summary>
        /// <param name="index">The index (0 being the top card) at which to cut the deck. 
        /// Not providing this parameter(or making it negative) causes the cut to be placed at some random position. </param>
        /// 
        public void Cut(int index = -1) {
            if(index > _masterDeck.Count-1) throw new IndexOutOfRangeException("Tried to cut the deck at an invalid index.");

            if (index < 0) index = Rng.Next(0, _masterDeck.Count);

            lock (Locker) {
                var cardList = _masterDeck.ToList();
                var firstPart = cardList.GetRange(0, index + 1);
                var secondPart = cardList.GetRange(index + 1, cardList.Count - firstPart.Count);

                cardList = secondPart;
                cardList.AddRange(firstPart);

                ConvertListToDeck(cardList);
            }
        }

        /// <summary>
        /// Clears deck and reinitializes it with the cards in the provided list.
        /// </summary>
        /// <param name="newDeck">The list that represents the quantity and ordering of the deck.</param>
        private void ConvertListToDeck(List<Card> newDeck)
        {
            if(newDeck == null) throw new ArgumentNullException(nameof(newDeck),"Deck to copy cannot be null.");
            lock (Locker)
            {
                _masterDeck.Clear();
                Push(newDeck);
            }
        }

        /// <summary>
        /// Constructor for the deck. Can specify size of deck. Extra cards are generated at random and added to the deck. 
        /// If deck is less than 52 cards, higher rank/value cards will be excluded.
        /// </summary>
        /// <param name="deckSize">How many cards to include in this deck</param>
        /// <param name="createExtraCardsRandomly">True: Create extra cards at random. False: Create extra cards sequentially (useful for creating double decks)</param>
        public Deck(int deckSize = 52, bool createExtraCardsRandomly = false)
        {
            if (deckSize < 0) throw new ArgumentException("Deck cannot have less than 0 cards.");
            //52 cards.
            foreach (CardSuit face in Enum.GetValues(typeof(CardSuit))) {
                    foreach (CardValue value in Enum.GetValues(typeof(CardValue))) {
                        lock(Locker) { _masterDeck.Push(new Card(face, value));}
                    }
            }
            //<52 cards... higher rank/value will be skipped...
            if (deckSize < 52) {
                var masterList = _masterDeck.ToList();
                lock (Locker) {
                    _masterDeck.Clear();
                    masterList.GetRange(0, deckSize).ForEach(p => {
                        _masterDeck.Push(p);
                    });
                }
            }
            //>52 cards... allows for random extra card creation or sequential
            else if (deckSize > 52)
            {
                lock (Locker) {
                    if(createExtraCardsRandomly)
                        for (var i = 0; i < deckSize - 52; i++) {
                            _masterDeck.Push(new Card((CardSuit)Rng.Next(4), (CardValue)Rng.Next(13)));
                        }
                    else {
                        var i = 0;
                        foreach (CardSuit face in Enum.GetValues(typeof(CardSuit))) {
                            foreach (CardValue value in Enum.GetValues(typeof(CardValue))) {
                                if(++i > deckSize - 52) return;
                                lock (Locker) { _masterDeck.Push(new Card(face, value)); }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="other">The other deck that we are copying from</param>
        public Deck(Deck other) {
            if(other==null) throw new ArgumentNullException(nameof(other),"Deck to copy cannot be null.");
            lock (Locker) {
                foreach (var card in other) {
                    if (card.CurrentlyInDeck) _masterDeck.Push(card);
                    else _drawnCards.Add(card);
                }
            }
            
        }

        /// <summary>
        /// Number of drawable cards currently in the deck.
        /// </summary>
        public int CurrentCount => _masterDeck.Count;

        /// <summary>
        /// Number of cards in the deck, regardless as to whether they have been drawn yet or not. This 
        /// should not change, as we never actually remove or add cards to the deck- we simply mark them as 'drawable' or not.
        /// </summary>
        public int TotalCount => _masterDeck.Count + _drawnCards.Count;

        /// <summary>
        /// List of cards that have been drawn.
        /// </summary>
        public IEnumerable<Card> DrawnCards => _drawnCards;

        /// <summary>
        /// Number of cards currently drawn from the deck.
        /// </summary>
        public int DrawnCount => _drawnCards.Count;

        //Our deck
        private readonly Stack<Card> _masterDeck = new Stack<Card>();

        //Cards that are not currently in the deck
        private readonly List<Card> _drawnCards = new List<Card>();

        private static readonly Random Rng = new Random();
        private static readonly object Locker = new object();

        public IEnumerator<Card> GetEnumerator() {
            lock (Locker) {
                return _masterDeck.GetEnumerator();
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
