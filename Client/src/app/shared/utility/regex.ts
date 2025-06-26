export class Regex {
    // Basic email validation
  static email = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  // 10-digit phone number (India/US-style)
  static phone = /^[0-9]{10}$/;

  // Strong password: min 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char
  static strongPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+=\-])[A-Za-z\d@$!%*?&#^()_+=\-]{8,}$/;

  // URL validation (basic)
  static url = /^(https?:\/\/)?([\w\-]+\.)+[a-z]{2,6}([\/\w\-._~:?#[\]@!$&'()*+,;=]*)*\/?$/i;

  // Only alphabets (no spaces)
  static alphabetOnly = /^[A-Za-z]+$/;

  // Alphabets and spaces (e.g., for names)
  static alphabetWithSpaces = /^[A-Za-z\s]+$/;

  // Alphanumeric (no special chars)
  static alphaNumeric = /^[A-Za-z0-9]+$/;

  // Alphanumeric with spaces
  static alphaNumericWithSpaces = /^[A-Za-z0-9\s]+$/;

  // Decimal number with optional 2 digits after decimal
  static decimal2 = /^\d+(\.\d{1,2})?$/;

  // Date in YYYY-MM-DD format
  static dateYYYYMMDD = /^\d{4}-\d{2}-\d{2}$/;

  // Indian postal code (PIN)
  static indianPIN = /^[1-9][0-9]{5}$/;

  // Indian Aadhaar number (12-digit)
  static aadhaar = /^\d{4}\s\d{4}\s\d{4}$/;

  // PAN card (India): 5 letters, 4 digits, 1 letter
  static panCard = /^[A-Z]{5}[0-9]{4}[A-Z]$/;

  // GSTIN (India): 15-character alphanumeric
  static gstin = /^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/;
}