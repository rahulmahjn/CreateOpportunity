using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOpportunity.DAL
{
    public class Utility
    {
        public static string GetCountryCode(string country)
        {
            var countryCodesMapping = new Dictionary<string, string>() {
                           { "Afghanistan", "AF" },    // Afghanistan
                           { "Albania", "AL" },    // Albania
                           { "Algeria", "DZ" },    // Argentina
                           { "American Samoa", "AS" },    // Armenia
                           { "Andorra", "AD" },    // Australia
                           { "Angola", "AO" },    // Austria
                           { "Anguilla", "AI" },    // Azerbaijan
                           { "Antarctica", "AQ" },    // Belgium
                           { "Antigua and Barbuda", "AG" },    // Bangladesh
                           { "Argentina", "AR" },    // Bulgaria
                           { "Armenia", "AM" },    // Bahrain
                           { "Aruba", "AW" },    // Bosnia and Herzegovina
                           { "Australia", "AU" },    // Belarus
                           { "Austria", "AT" },    // Belize
                           { "Azerbaijan", "AZ" },    // Bolivia
                           { "Bahamas", "BS" },    // Brazil
                           { "Bahrain", "BH" },    // Canada
                           { "Bangladesh", "BD" },    // Switzerland
                           { "Barbados", "BB" },    // Chile
                           { "Belarus", "BY" },    // People's Republic of China
                           { "Belgium", "BE" },    // Colombia
                           { "Belize", "BZ" },    // Costa Rica
                           { "Benin", "BJ" },    // Czech Republic
                           { "Bermuda", "BM" },    // Germany
                           { "Bhutan", "BT" },    // Denmark
                           { "Bolivia", "BO" },    // Dominican Republic
                           { "Bonaire", "BQ" },    // Algeria
                           { "Bosnia and Herzegovina", "BA" },    // Ecuador
                           { "Botswana", "BW" },    // Egypt
                           { "Bouvet Island", "BV" },    // Spain
                           { "Brazil", "BR" },    // Estonia
                           { "British Indian Ocean Territory", "IO" },    // Ethiopia
                           { "Brunei Darussalam", "BN" },    // Finland
                           { "Bulgaria", "BG" },    // France
                           { "Burkina Faso", "BF" },    // Faroe Islands
                           { "Burundi", "BI" },    // United Kingdom
                           { "Cabo Verde", "CV" },    // Georgia
                           { "Cambodia", "KH" },    // Greece
                           { "Cameroon", "CM" },    // Greenland
                           { "Canada", "CA" },    // Guatemala
                           { "Cayman Islands", "KY" },    // Hong Kong S.A.R.
                           { "Central African Republic", "CF" },    // Honduras
                           { "Chad", "TD" },    // Croatia
                           { "Chile", "CL" },    // Hungary
                           { "China", "CN" },    // Indonesia
                           { "Christmas Island", "CX" },    // India
                           { "Cocos", "CC" },    // Ireland
                           { "Colombia", "CO" },    // Iran
                           { "Comoros", "KM" },    // Iraq
                           { "Congo", "CD" },    // Iceland
                           { "Cook Islands", "CK" },    // Israel
                           { "Costa Rica", "CR" },    // Italy
                           { "Croatia", "CU" },    // Jamaica
                           { "Cuba", "CU" },    // Jordan
                           { "Curaçao", "CW" },    // Japan
                           { "Cyprus", "CY" },    // Kazakhstan
                           { "Czechia", "CZ" },    // Kenya
                           { "Côte d'Ivoire", "CI" },    // Kyrgyzstan
                           { "Denmark", "DK" },    // Cambodia
                           { "Djibouti", "DJ" },    // Korea
                           { "Dominica", "DM" },    // Kuwait
                           { "Dominican Republic", "DO" },    // Lao P.D.R.
                           { "Ecuador", "EC" },    // Lebanon
                           { "Egypt", "EG" },    // Libya
                           { "El Salvador", "SV" },    // Liechtenstein
                           { "Equatorial Guinea", "GQ" },    // Sri Lanka
                           { "Eritrea", "ER" },    // Lithuania
                           { "Estonia", "EE" },    // Luxembourg
                           { "Eswatini", "SZ" },    // Latvia
                           { "Ethiopia", "ET" },    // Macao S.A.R.
                           { "Falkland Islands", "FK" },    // Morocco
                           { "Faroe Islands", "FO" },    // Principality of Monaco
                           { "Fiji", "FJ" },    // Maldives
                           { "Finland", "FI" },    // Mexico
                           { "France", "FR" },    // Macedonia (FYROM)
                           { "French Guiana", "GF" },    // Malta
                           { "French Polynesia", "PF" },    // Montenegro
                           { "French Southern Territories", "TF" },    // Mongolia
                           { "Gabon", "GA" },    // Malaysia
                           { "Gambia ", "GM" },    // Nigeria
                           { "Georgia", "GE" },    // Nicaragua
                           { "Germany", "DE" },    // Netherlands
                           { "Ghana", "GH" },    // Norway
                           { "Gibraltar", "GI" },    // Nepal
                           { "Greece", "GR" },    // New Zealand
                           { "Greenland", "GL" },    // Oman
                           { "Grenada", "GD" },    // Islamic Republic of Pakistan
                           { "Guadeloupe", "GP" },    // Panama
                           { "Guam", "GU" },    // Peru
                           { "Guatemala", "GT" },    // Republic of the Philippines
                           { "Guernsey", "GG" },    // Poland
                           { "Guinea", "GN" },    // Puerto Rico
                           { "Guinea-Bissau", "GW" },    // Portugal
                           { "Guyana", "GY" },    // Paraguay
                           { "Haiti", "HT" },    // Qatar
                           { "Heard Island and McDonald Islands", "HM" },    // Romania
                           { "Holy See", "VA" },    // Russia
                           { "Honduras", "HN" },    // Rwanda
                           { "Hong Kong", "HK" },    // Saudi Arabia
                           { "Hungary", "HU" },    // Serbia and Montenegro (Former)
                           { "Iceland", "IS" },    // Senegal
                           { "India", "IN" },    // Singapore
                           { "Indonesia", "ID" },    // El Salvador
                           { "Iran ", "IR" },    // Serbia
                           { "Iraq", "IQ" },    // Slovakia
                           { "Ireland", "IE" },    // Slovenia
                           { "Isle of Man", "IM" },    // Sweden
                           { "Israel", "IL" },    // Syria
                           { "Italy", "IT" },    // Tajikistan
                           { "Jamaica", "JM" },    // Thailand
                           { "Japan", "JP" },    // Turkmenistan
                           { "Jersey", "JE" },    // Trinidad and Tobago
                           { "Jordan", "JO" },    // Tunisia
                           { "Kazakhstan", "KZ" },    // Turkey
                           { "Kenya", "KE" },    // Taiwan
                           { "Kiribati", "KI" },    // Ukraine
                           { "Korea ", "KR" },    // Uruguay
                           { "Kuwait", "KW" },    // United States
                           { "Kyrgyzstan", "KG" },    // Uzbekistan
                           { "Lao People's Democratic Republic", "LA" },    // Bolivarian Republic of Venezuela
                           { "Latvia", "LV" },    // Vietnam
                           { "Lebanon", "LB" },    // Yemen
                           { "Lesotho", "LS" },    // South Africa
                           { "Liberia", "LR" },    // Zimbabwe
                           { "Libya", "LY" },    // Slovakia
                           { "Liechtenstein", "LI" },    // Slovenia
                           { "Lithuania", "LT" },    // Sweden
                           { "Luxembourg", "LU" },    // Syria
                           { "Macao", "MO" },    // Tajikistan
                           { "Madagascar", "MG" },    // Thailand
                           { "Malawi", "MW" },    // Turkmenistan
                           { "Malaysia", "MY" },    // Trinidad and Tobago
                           { "Maldives", "MV" },    // Tunisia
                           { "Mali", "ML" },    // Turkey
                           { "Malta", "MT" },    // Taiwan
                           { "Marshall Islands", "MH" },    // Ukraine
                           { "Martinique", "MQ" },    // Uruguay
                           { "Mauritania", "MR" },    // United States
                           { "Mauritius", "MU" },    // Uzbekistan
                           { "Mayotte", "YT" },    // Bolivarian Republic of Venezuela
                           { "Mexico", "MX" },    // Vietnam
                           { "Micronesia ", "FM" },    // Yemen
                           { "Moldova ", "MD" },    // South Africa
                           { "Monaco", "MC" },    // Zimbabwe
                           { "Mongolia", "MN" },    // Slovakia
                           { "Montenegro", "ME" },    // Slovenia
                           { "Montserrat", "MS" },    // Sweden
                           { "Morocco", "MA" },    // Syria
                           { "Mozambique", "MZ" },    // Tajikistan
                           { "Myanmar", "MM" },    // Thailand
                           { "Namibia", "NA" },    // Turkmenistan
                           { "Nauru", "NR" },    // Trinidad and Tobago
                           { "Nepal", "NP" },    // Tunisia
                           { "Netherlands", "NL" },    // Turkey
                           { "New Caledonia", "NC" },    // Taiwan
                           { "New Zealand", "NZ" },    // Ukraine
                           { "Nicaragua", "NI" },    // Uruguay
                           { "Niger ", "NE" },    // United States
                           { "Nigeria", "NG" },    // Uzbekistan
                           { "Norfolk Island", "NF" },    // Bolivarian Republic of Venezuela
                           { "Northern Mariana Islands", "MP" },    // Vietnam
                           { "Norway", "NO" },    // Yemen
                           { "Oman", "OM" },    // South Africa
                           { "Pakistan", "PK" },    // Zimbabwe
                           { "Palau", "PW" },    // Uruguay
                           { "Palestine, State of", "PS" },    // United States
                           { "Panama", "PA" },    // Uzbekistan
                           { "Papua New Guinea", "PG" },    // Bolivarian Republic of Venezuela
                           { "Paraguay", "PY" },    // Vietnam
                           { "Peru", "PE" },    // Yemen
                           { "Philippines ", "PH" },    // South Africa
                           { "Pitcairn", "PN" },    // Zimbabwe
                           { "Poland", "PL" },    // Uruguay
                           { "Portugal", "PT" },    // United States
                           { "Puerto Rico", "PR" },    // Uzbekistan
                           { "Qatar", "QA" },    // Bolivarian Republic of Venezuela
                           { "Republic of North Macedonia", "MK" },    // Vietnam
                           { "Romania", "RO" },    // Yemen
                           { "Russia", "RU" },    // South Africa
                           { "Rwanda", "RW" },    // Zimbabwe
                           { "Réunion", "RE" },    // Uruguay
                           { "Saint Barthélemy", "BL" },    // United States
                           { "Saint Helena, Ascension and Tristan da Cunha", "SH" },    // Uzbekistan
                           { "Saint Kitts and Nevis", "KN" },    // Bolivarian Republic of Venezuela
                           { "Saint Lucia", "LC" },    // Vietnam
                           { "Saint Martin", "MF" },    // Yemen
                           { "Saint Pierre and Miquelon", "PM" },    // South Africa
                           { "Saint Vincent and the Grenadines", "VC" },    // Zimbabwe
                           { "Samoa", "WS" },    // Uruguay
                           { "San Marino", "SM" },    // United States
                           { "Sao Tome and Principe", "ST" },    // Uzbekistan
                           { "Saudi Arabia", "SA" },    // Bolivarian Republic of Venezuela
                           { "Senegal", "SN" },    // Vietnam
                           { "Serbia", "RS" },    // Yemen
                           { "Seychelles", "SC" },    // South Africa
                           { "Sierra Leone", "SL" },    // Zimbabwe
                           { "Singapore", "SG" },    // Uruguay
                           { "Sint Maarten", "SX" },    // United States
                           { "Slovakia", "SK" },    // Uzbekistan
                           { "Slovenia", "SI" },    // Bolivarian Republic of Venezuela
                           { "Solomon Islands", "SB" },    // Vietnam
                           { "Somalia", "SO" },    // Yemen
                           { "South Africa", "ZA" },    // South Africa
                           { "South Georgia and the South Sandwich Islands", "GS" },    // Zimbabwe
                           { "South Sudan", "SS" },    // Vietnam
                           { "Spain", "ES" },    // Yemen
                           { "Sri Lanka", "LK" },    // South Africa
                           { "Sudan ", "SD" },    // Zimbabwe
                           { "Suriname", "SR" },    // Vietnam
                           { "Svalbard and Jan Mayen", "SJ" },    // Yemen
                           { "Sweden", "SE" },    // South Africa
                           { "Switzerland", "CH" },    // Zimbabwe
                           { "Syrian Arab Republic", "SY" },    // Vietnam
                           { "Taiwan ", "TW" },    // Yemen
                           { "Tajikistan", "TJ" },    // South Africa
                           { "Tanzania", "TZ" },    // Zimbabwe
                           { "Thailand", "TH" },    // Vietnam
                           { "Timor-Leste", "TL" },    // Yemen
                           { "Togo", "TG" },    // South Africa
                           { "Tokelau", "TK" },    // Zimbabwe
                           { "Tonga", "TO" },    // Vietnam
                           { "Trinidad and Tobago", "TT" },    // Yemen
                           { "Tunisia", "TN" },    // South Africa
                           { "Turkey", "TR" },    // Vietnam
                           { "Turkmenistan", "TM" },    // Yemen
                           { "Turks and Caicos Islands", "TC" },    // South Africa
                           { "Tuvalu", "TV" },    // Zimbabwe
                           { "Uganda", "UG" },    // Vietnam
                           { "Ukraine", "UA" },    // Yemen
                           { "United Arab Emirates", "AE" },    // South Africa
                           { "UAE", "AE" },
                           { "United Kingdom of Great Britain and Northern Ireland", "GB" },    // Zimbabwe
                           { "United Kingdom of Great Britain", "GB" },    // Zimbabwe
                           { "United Kingdom", "GB" },    // Yemen
                           { "UK", "GB" },    // South Africa
                           { "England", "GB" },    // Zimbabwe
                           { "Wales", "GB" },    // Zimbabwe
                           { "Scotland", "GB" },    // Yemen
                           { "Nothern Ireland", "GB" },    // South Africa
                           { "United States Minor Outlying Islands", "US" },    // Zimbabwe
                           { "United States of America", "US" },    // Zimbabwe
                           { "USA", "US" },    // Yemen
                           { "Uruguay", "UY" },    // South Africa
                           { "Uzbekistan", "UZ" },    // Zimbabwe
                           { "Vanuatu", "VU" },    // Zimbabwe
                           { "Venezuela ", "VE" },    // Zimbabwe
                           { "Viet Nam", "VN" },    // Zimbabwe
                           { "Virgin Islands (British)", "VG" },    // Yemen
                           { "Virgin Islands (U.S.)", "VN" },    // South Africa
                           { "Wallis and Futuna", "WF" },    // Zimbabwe
                           { "Western Sahara", "EH" },    // Zimbabwe
                           { "Yemen", "YE" },    // Zimbabwe
                           { "Zambia", "ZM" },    // Zimbabwe
                           { "Zimbabwe", "ZW" },    // Yemen
                           { "Åland Islands", "AX" }    // South Africa             
                        };
            
            return countryCodesMapping[country];
        }
    }
}
