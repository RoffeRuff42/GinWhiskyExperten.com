# GinWhiskyExperten.com

GinWhiskyExperten is a REST API designed to help enthusiasts find bottles that match their specific taste preferences. By tracking flavors and their intensities, the system provides personalized recommendations and allows users to curate their own collection.

## User Flows
1. **Taste Discovery (GET):** Users can filter spirits by type (Gin/Whisky), alcohol percentage, and specific flavors (e.g., "Smokey" or "Floral"). Results are paginated for performance.
2. **Library Management (POST/PUT):** Administrators can add new bottles to the database, assign them to distilleries, and define their flavor profiles.
3. **Personalization (POST):** Users can rate bottles and save them to their personal "library" (wishlist).
4. **Smart Match (Logic):** Based on a specific bottle, the API suggests similar spirits by calculating the distance between flavor intensities.
