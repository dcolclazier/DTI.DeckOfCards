
namespace DSI.Deck
{
    /// <summary>
    /// Represents a card with a particular suit and value.
    /// <example>var aceUpMySleeve = new Card(CardValue.Ace, CardSuit.Spades);</example>
    /// </summary>
    public class Card 
    {
        /// <summary>
        /// Default constructor for a card - must provide suit and value.
        /// </summary>
        /// <param name="suit">The card's suit</param>
        /// <param name="value">The card's face value</param>
        public Card(CardSuit suit, CardValue value) {
            Value = value;
            Suit = suit;
            CurrentlyInDeck = true;
        }

        /// <summary>
        /// Compares one card to another for equality
        /// </summary>
        /// <param name="other">The card to compare this one to</param>
        /// <returns></returns>
        public bool Equals(Card other) {
            return Value == other?.Value && Suit == other.Suit;
        }

        /// <summary>
        /// Denotes the card as 'drawn' so it cannot be drawn again.
        /// </summary>
        public void DrawFromDeck() => CurrentlyInDeck = false;

        /// <summary>
        /// Returns the card to the deck so it may be drawn again.
        /// </summary>
        public void ReturnToDeck() => CurrentlyInDeck = true;

        /// <summary>
        /// True, if the card is currently in the deck
        /// </summary>
        public bool CurrentlyInDeck { get; private set; }

        /// <summary>
        /// This card's face value
        /// </summary>
        public CardValue Value { get; }

        /// <summary>
        /// This card's suit
        /// </summary>
        public CardSuit Suit { get; }

        /// <summary>
        /// A nicely formatted string to identify this card.
        /// </summary>
        public string FullName => Value + " of " + Suit;
    }

    
}
