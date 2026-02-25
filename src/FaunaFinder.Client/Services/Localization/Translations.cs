namespace FaunaFinder.Client.Services.Localization;

public static class Translations
{
    public static IReadOnlyDictionary<string, string> English { get; } =
        new Dictionary<string, string>
        {
            // Navigation
            ["Nav_Map"] = "Map",
            ["Nav_Species"] = "Species",
            ["Nav_Pueblos"] = "Municipalities",
            ["Nav_About"] = "About",
            ["Nav_Sightings"] = "Sightings",
            ["Nav_Dashboard"] = "Dashboard",

            // Common
            ["AppTitle"] = "FaunaFinder",
            ["Loading"] = "Loading...",
            ["LoadMore"] = "Load More",
            ["ShowLess"] = "Show Less",
            ["Back"] = "Back",
            ["TryAgain"] = "Try Again",
            ["Close"] = "Close",
            ["ViewDetails"] = "View Details",
            ["ViewAllSpecies"] = "View All Species",
            ["ViewAll"] = "View All",
            ["AllSpecies"] = "All Species",
            ["Details"] = "Details",

            // Errors
            ["Error_SomethingWentWrong"] = "Something went wrong",
            ["Error_UnexpectedError"] = "An unexpected error occurred. Please try again.",
            ["Error_SpeciesNotFound"] = "Species not found.",
            ["Error_MunicipalityNotFound"] = "Municipality not found.",
            ["MunicipalityNotFound"] = "Municipality '{0}' not found in database.",

            // Map Page
            ["Map_Loading"] = "Loading map...",
            ["Map_ClickMunicipality"] =
                "Click on a municipality on the map to view species and conservation information.",
            ["Map_NoSpeciesData"] = "No species data available for this municipality.",
            ["Map_SpeciesFound"] = "{0} species found",
            ["Map_SpeciesInDatabase"] = "{0} species in database",
            ["Map_ConservationLinks"] = "Conservation Links",
            ["Map_ClearLocations"] = "Clear",
            ["Map_LocationsFound"] = "{0} location(s) found",
            ["Map_ViewAllLocations"] = "View All Locations",
            ["Map_BackToSpecies"] = "Back to Species Details",
            ["Map_UnnamedLocation"] = "Location",

            // Species Page
            ["Species_Title"] = "Species",
            ["Species_Description"] =
                "Explore the species of Puerto Rico and discover where they can be found.",
            ["Species_SearchPlaceholder"] = "Search species...",
            ["Species_NoResults"] = "No species found matching your search.",
            ["Species_Municipality"] = "municipality",
            ["Species_Municipalities"] = "municipalities",
            ["Species_Showing"] = "Showing {0}-{1} of {2} species",

            // Conservation Status
            ["Conservation_CriticallyImperiled"] = "Critically Imperiled",
            ["Conservation_Imperiled"] = "Imperiled",
            ["Conservation_Vulnerable"] = "Vulnerable",
            ["Conservation_ApparentlySecure"] = "Apparently Secure",
            ["Conservation_Secure"] = "Secure",

            // Stats Hero
            ["Stats_Species"] = "Species",
            ["Stats_Municipalities"] = "Municipalities",
            ["Stats_Sightings"] = "Sightings",

            // Species Detail Page
            ["SpeciesDetail_FoundIn"] = "Found in {0} {1}",
            ["SpeciesDetail_MunicipalitiesTitle"] = "Municipalities",
            ["SpeciesDetail_NoMunicipalityData"] =
                "No municipality data available for this species.",
            ["SpeciesDetail_ConservationLinksTitle"] = "Conservation Links",
            ["SpeciesDetail_NoConservationLinks"] =
                "No conservation links available for this species.",
            ["SpeciesDetail_ViewLocations"] = "View Locations",
            ["SpeciesDetail_ImageSource"] = "Image source",

            // Pueblos Page
            ["Pueblos_Title"] = "Municipalities of Puerto Rico",
            ["Pueblos_Description"] =
                "Explore the municipalities of Puerto Rico and discover their biodiversity.",
            ["Pueblos_SearchPlaceholder"] = "Search municipalities...",
            ["Pueblos_NoResults"] = "No municipalities found matching your search.",
            ["Pueblos_Species"] = "species",
            ["Pueblos_Showing"] = "Showing {0}-{1} of {2} municipalities",

            // Pueblo Detail Page
            ["PuebloDetail_SpeciesInMunicipality"] = "Species in this Municipality",
            ["PuebloDetail_NoSpeciesData"] = "No species data available for this municipality.",
            ["PuebloDetail_NoConservationLinks"] =
                "No conservation links available for this species.",
            ["PuebloDetail_ViewLocation"] = "View on Map",

            // About Page
            ["About_Title"] = "About FaunaFinder",
            ["About_WhatIsTitle"] = "What is FaunaFinder?",
            ["About_WhatIsDescription"] =
                "FaunaFinder is an interactive web application that helps users explore conservation information for Puerto Rico's municipalities. Click on any municipality on the map to discover the species that inhabit that region, along with relevant NRCS conservation practices and FWS action recommendations.",
            ["About_DataSourcesTitle"] = "Data Sources",
            ["About_NrcsPractices"] = "NRCS Practices:",
            ["About_NrcsPracticesDesc"] =
                "Natural Resources Conservation Service conservation practice standards",
            ["About_FwsActions"] = "FWS Actions:",
            ["About_FwsActionsDesc"] =
                "U.S. Fish and Wildlife Service recommended conservation actions",
            ["About_SpeciesData"] = "Species Data:",
            ["About_SpeciesDataDesc"] =
                "Species occurrence and habitat information for Puerto Rico",
            ["About_VisitNrcs"] = "Visit NRCS Practice Standards",
            ["About_VisitEcos"] = "Visit ECOS Species Profiles",
            ["About_VisitFwsCaribbean"] = "Visit FWS Caribbean",
            ["About_AcknowledgmentsTitle"] = "Acknowledgments",
            ["About_AcknowledgmentsDesc"] =
                "FaunaFinder was built using publicly available conservation data and open-source technologies.",
            ["About_SpeciesImages"] = "Species Images:",
            ["About_SpeciesImagesDesc"] =
                "Profile images are sourced from various public domain and Creative Commons sources, with attribution shown on each species detail page.",

            // Filter and Sort
            ["Filter_Sort"] = "Sort",
            ["Filter_NameAZ"] = "Name (A-Z)",
            ["Filter_NameZA"] = "Name (Z-A)",
            ["Filter_ScientificAZ"] = "Scientific (A-Z)",
            ["Filter_ScientificZA"] = "Scientific (Z-A)",
            ["Filter_Filters"] = "Filters",
            ["Filter_ClearAll"] = "Clear All",
            ["Filter_Apply"] = "Apply",
            ["Filter_NrcsPractices"] = "NRCS Practices",
            ["Filter_FwsActions"] = "FWS Actions",
            ["Filter_ShowingFiltered"] = "Showing {0} of {1} species",
            ["Filter_ShowingFilteredLinks"] = "Showing {0} of {1} links",
            ["Filter_NoMatches"] = "No species match the selected filters.",
            ["Filter_NoMatchesLinks"] = "No conservation links match the selected filters.",
            ["Filter_SearchLinks"] = "Search conservation links...",

            // Page Titles
            ["PageTitle_Map"] = "FaunaFinder - Puerto Rico",
            ["PageTitle_Species"] = "Species - FaunaFinder",
            ["PageTitle_Pueblos"] = "Municipalities - FaunaFinder",
            ["PageTitle_About"] = "About - FaunaFinder",

            // Species Near Me
            ["NearMe_Title"] = "Species Near Me",
            ["NearMe_Button"] = "Near Me",
            ["NearMe_SelectRadius"] = "Select search radius",
            ["NearMe_Searching"] = "Searching for species...",
            ["NearMe_NoSpeciesFound"] =
                "No species found within this radius. Try expanding the search area.",
            ["NearMe_SpeciesFound"] = "{0} species found within {1}km",
            ["NearMe_UseLocateFirst"] =
                "Use the locate button on the map to enable species search near your location.",
            ["NearMe_ShowLocations"] = "Show Locations",
            ["NearMe_HideLocations"] = "Hide Locations",

            // Draw Polygon Search
            ["DrawPolygon_Title"] = "Custom Area",
            ["DrawPolygon_Button"] = "Draw Shape",
            ["DrawPolygon_Tooltip"] = "Draw a custom shape on the map to search for species",
            ["DrawPolygon_Instructions"] = "Click to add points",
            ["DrawPolygon_Redraw"] = "Redraw Shape",
            ["DrawPolygon_Finish"] = "Finish",
            ["DrawPolygon_SpeciesFound"] = "{0} species found in selected area",

            // Heatmap
            ["Heatmap_Toggle"] = "Heatmap",
            ["Heatmap_Tooltip"] = "Show species density heatmap",
            ["Heatmap_Loading"] = "Loading heatmap data...",
            ["Heatmap_FilterAll"] = "All",
            ["Heatmap_FilterFauna"] = "Fauna",
            ["Heatmap_FilterFlora"] = "Flora",

            // Export
            ["Export_Button"] = "Export",
            ["Export_PDF"] = "Download PDF",
            ["Export_CSV"] = "Download CSV",
            ["Export_Generating"] = "Generating report...",

            // Login
            ["Login_Title"] = "Sign In",
            ["Login_Description"] = "Sign in to access contributor features.",
            ["Login_SignIn"] = "Sign In",
            ["Login_Email"] = "Email",
            ["Login_Password"] = "Password",
            ["Login_RememberMe"] = "Remember me",
            ["Login_EmailRequired"] = "Email is required",
            ["Login_PasswordRequired"] = "Password is required",
            ["Login_AlreadyLoggedIn"] = "You are signed in as {0}",
            ["Login_GoToHome"] = "Go to Home",
            ["Login_SignOut"] = "Sign Out",
            ["Login_Profile"] = "Profile",
            ["Login_PendingApproval"] = "Your account is pending approval",
            ["Login_NoAccount"] = "Don't have an account? ",
            ["Login_CreateAccount"] = "Create one",

            // Register
            ["Register_Title"] = "Create Account",
            ["Register_Description"] = "Create an account to contribute to FaunaFinder.",
            ["Register_DisplayName"] = "Display Name",
            ["Register_DisplayNameRequired"] = "Display name is required",
            ["Register_ConfirmPassword"] = "Confirm Password",
            ["Register_ConfirmPasswordRequired"] = "Please confirm your password",
            ["Register_PasswordRequirements"] =
                "At least 8 characters with uppercase, lowercase, and number",
            ["Register_PasswordMismatch"] = "Passwords do not match",
            ["Register_CreateAccount"] = "Create Account",
            ["Register_HasAccount"] = "Already have an account? ",
            ["Register_Success"] =
                "Account created successfully! Your account is pending approval.",
            ["Register_Message"] = "Message (optional)",
            ["Register_MessageHelper"] = "Tell us why you'd like access",
            ["Register_SuccessPending"] =
                "Registration submitted! An admin will review your request.",

            // Admin
            ["Admin_Title"] = "User Management",
            ["Admin_Description"] = "Manage user accounts and permissions.",
            ["Admin_NoUsers"] = "No users found.",
            ["Admin_Email"] = "Email",
            ["Admin_DisplayName"] = "Name",
            ["Admin_Status"] = "Status",
            ["Admin_Role"] = "Role",
            ["Admin_CreatedAt"] = "Created",
            ["Admin_Actions"] = "Actions",
            ["Admin_Approve"] = "Approve",
            ["Admin_Reject"] = "Reject",
            ["Admin_MakeAdmin"] = "Make Admin",
            ["Admin_MakeContributor"] = "Make Contributor",
            ["Admin_MakeViewer"] = "Make Viewer",
            ["Admin_Pending"] = "Pending",
            ["Admin_Approved"] = "Approved",
            ["Admin_Rejected"] = "Rejected",
            ["Admin_Viewer"] = "Viewer",
            ["Admin_Contributor"] = "Contributor",
            ["Admin_Admin"] = "Admin",
            ["Admin_AccessDenied"] = "Access Denied",
            ["Admin_AccessDeniedMessage"] = "You do not have permission to access this page.",
            ["Admin_ViewDetails"] = "View Details",
            ["Admin_UserNotFound"] = "User not found.",
            ["Admin_UserInfo"] = "User Information",
            ["Admin_PendingActionDescription"] =
                "This user is awaiting approval. Approve or reject their account request.",
            ["Admin_NoActionsAvailable"] = "No actions available for this user.",
            ["Admin_AccessRequests"] = "Access Requests",
            ["Admin_AccessRequestDetails"] = "Access Request Details",
            ["Admin_Message"] = "Message",
            ["Admin_NoAccessRequests"] = "No pending access requests.",
            ["Admin_AccessRequests_Title"] = "Access Requests",
            ["Admin_AccessRequests_Description"] = "Review and manage user registration requests.",
            ["Admin_AccessRequests_SearchPlaceholder"] = "Search by name or email...",
            ["Admin_AccessRequests_StatusFilter"] = "Status",
            ["Admin_AccessRequests_StatusAll"] = "All",
            ["Nav_Admin"] = "Admin",

            // User Menu
            ["UserMenu_Logout"] = "Sign Out",

            // Wildlife Sighting
            ["Sighting_ReportTitle"] = "Report Wildlife Sighting",
            ["Sighting_Mode"] = "Sighting Mode",
            ["Sighting_ModeCasual"] = "Casual - Anytime sighting",
            ["Sighting_ModeSurvey"] = "Survey - Systematic search",
            ["Sighting_Species"] = "Species",
            ["Sighting_SearchSpecies"] = "Search Species",
            ["Sighting_SearchPlaceholder"] = "Start typing to search...",
            ["Sighting_SpeciesRequired"] = "Species is required",
            ["Sighting_ObservationDetails"] = "Observation Details",
            ["Sighting_Confidence"] = "Confidence Level",
            ["Sighting_ConfidenceCertain"] = "Certain - Definitely identified",
            ["Sighting_ConfidenceFairlySure"] = "Fairly Sure - Likely correct",
            ["Sighting_ConfidenceUnsure"] = "Unsure - Best guess",
            ["Sighting_Count"] = "Count Estimate",
            ["Sighting_Behavior"] = "Behavior Observed",
            ["Sighting_BehaviorFeeding"] = "Feeding",
            ["Sighting_BehaviorResting"] = "Resting",
            ["Sighting_BehaviorMoving"] = "Moving",
            ["Sighting_BehaviorCalling"] = "Calling",
            ["Sighting_Evidence"] = "Evidence Type",
            ["Sighting_EvidenceVisual"] = "Visual",
            ["Sighting_EvidenceHeard"] = "Heard",
            ["Sighting_EvidenceTracks"] = "Tracks/Signs",
            ["Sighting_EvidencePhoto"] = "Photo",
            ["Sighting_EvidenceRequired"] = "At least one evidence type is required",
            ["Sighting_Weather"] = "Weather (optional)",
            ["Sighting_WeatherClear"] = "Clear",
            ["Sighting_WeatherPartlyCloudy"] = "Partly Cloudy",
            ["Sighting_WeatherCloudy"] = "Cloudy",
            ["Sighting_WeatherRainy"] = "Rainy",
            ["Sighting_WeatherStormy"] = "Stormy",
            ["Sighting_WeatherFoggy"] = "Foggy",
            ["Sighting_WeatherWindy"] = "Windy",
            ["Sighting_Notes"] = "Notes (optional)",
            ["Sighting_Location"] = "Location",
            ["Sighting_Latitude"] = "Latitude",
            ["Sighting_Longitude"] = "Longitude",
            ["Sighting_GetLocation"] = "Get Current Location",
            ["Sighting_ObservationTime"] = "Observation Time",
            ["Sighting_Date"] = "Date",
            ["Sighting_Time"] = "Time",
            ["Sighting_FutureDateError"] = "Observation date and time cannot be in the future",
            ["Sighting_Photo"] = "Photo (optional)",
            ["Sighting_UploadPhoto"] = "Upload Photo",
            ["Sighting_Submit"] = "Submit Sighting Report",
            ["Sighting_Unauthorized"] = "You must be logged in to submit a sighting",
            ["Sighting_UnexpectedError"] = "An unexpected error occurred. Please try again.",

            // My Sightings
            ["Sightings_MyTitle"] = "My Sightings",
            ["Sightings_Report"] = "Report",
            ["Sightings_NoSightings"] = "No sightings yet",
            ["Sightings_NoSightingsDescription"] =
                "You haven't reported any wildlife sightings yet.",
            ["Sightings_ReportFirst"] = "Report Your First Sighting",
            ["Sightings_UnknownSpecies"] = "Unknown Species",
            ["Sightings_Unauthorized"] = "You must be logged in to view your sightings",
            ["Sightings_StatusApproved"] = "Approved",
            ["Sightings_StatusRejected"] = "Rejected",
            ["Sightings_StatusPending"] = "Pending",

            // Sighting Detail
            ["SightingDetail_Title"] = "Sighting Details",
            ["SightingDetail_NotFound"] = "Sighting not found.",
            ["SightingDetail_Photo"] = "Photo",
            ["SightingDetail_PhotoAlt"] = "Sighting photo",
            ["SightingDetail_NoPhoto"] = "No photo available",
            ["SightingDetail_Location"] = "Location",
            ["SightingDetail_ObservationDetails"] = "Observation Details",
            ["SightingDetail_ObservedAt"] = "Observed",
            ["SightingDetail_ReportedAt"] = "Reported",
            ["SightingDetail_Weather"] = "Weather",
            ["SightingDetail_Notes"] = "Notes",
            ["SightingDetail_ReviewStatus"] = "Review Status",
            ["SightingDetail_ReviewedAt"] = "Reviewed",
            ["SightingDetail_ReviewNotes"] = "Review Notes",
            ["SightingDetail_FlaggedForReview"] =
                "This sighting has been flagged for expert review.",
            ["SightingDetail_NewMunicipalityRecord"] =
                "This is a new species record for this municipality!",
            ["SightingDetail_PendingReviewMessage"] =
                "Your sighting is awaiting review by a teacher or administrator.",

            // Sighting Detail Photo Upload
            ["SightingDetail_AddPhoto"] = "Add Photo",
            ["SightingDetail_ChangePhoto"] = "Change Photo",
            ["SightingDetail_UploadingPhoto"] = "Uploading...",
            ["SightingDetail_PhotoUploaded"] = "Photo uploaded successfully",
            ["SightingDetail_PhotoUploadError"] = "Failed to upload photo. Please try again.",
            ["SightingDetail_InvalidFileType"] =
                "Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.",
            ["SightingDetail_FileTooLarge"] = "File is too large. Maximum size is 5MB.",

            // Dashboard
            ["Dashboard_Title"] = "Dashboard",
            ["Dashboard_ReviewTab"] = "Review",
            ["Dashboard_AdminPanel_Title"] = "Administration",
            ["Dashboard_AdminPanel_Description"] =
                "Manage users, review requests, and oversee platform activity.",
            ["Dashboard_Card_UserManagement"] = "User Management",
            ["Dashboard_Card_UserManagement_Desc"] = "View, search, and manage all user accounts.",
            ["Dashboard_Card_UserRequests"] = "User Requests",
            ["Dashboard_Card_UserRequests_Desc"] =
                "Review and approve pending registration requests.",
            ["Dashboard_Card_UsersSightings"] = "Users Sightings",
            ["Dashboard_Card_UsersSightings_Desc"] = "View and manage sightings reported by users.",

            // Admin Users
            ["Admin_Users_SearchPlaceholder"] = "Search by name or email...",

            // Review Queue
            ["ReviewQueue_Title"] = "Review Queue",
            ["ReviewQueue_Empty"] = "No pending sightings to review",
            ["ReviewQueue_EmptyDescription"] =
                "All sightings have been reviewed. Check back later for new submissions.",
            ["ReviewQueue_Approve"] = "Approve",
            ["ReviewQueue_Reject"] = "Reject",
            ["ReviewQueue_ReviewNotes"] = "Review Notes (optional)",
            ["ReviewQueue_ReviewNotesPlaceholder"] = "Add notes about this review...",
            ["ReviewQueue_ApproveSuccess"] = "Sighting approved successfully",
            ["ReviewQueue_RejectSuccess"] = "Sighting rejected successfully",
            ["ReviewQueue_ReviewError"] = "Failed to review sighting. Please try again.",
            ["ReviewQueue_ObservedOn"] = "Observed",
            ["ReviewQueue_ReportedOn"] = "Reported",
            ["ReviewQueue_Location"] = "Location",
            ["ReviewQueue_ViewDetails"] = "View Details",
            ["ReviewQueue_CloseDetails"] = "Close",
            ["ReviewQueue_Unauthorized"] = "You do not have permission to access this page.",
            ["Nav_ReviewQueue"] = "Review Queue",

            // Statistics
            ["Nav_Statistics"] = "Statistics",
            ["PageTitle_Statistics"] = "Statistics - FaunaFinder",
            ["Statistics_Title"] = "Wildlife Statistics",
            ["Statistics_Description"] = "Explore aggregated data and trends from wildlife sightings across Puerto Rico.",
            ["Statistics_TotalSightings"] = "Total Sightings",
            ["Statistics_TotalSpecies"] = "Species in Database",
            ["Statistics_TotalMunicipalities"] = "Municipalities",
            ["Statistics_ThisMonth"] = "Sightings This Month",
            ["Statistics_SightingsOverTime"] = "Sightings Over Time",
            ["Statistics_SpeciesByCategory"] = "Species by Category",
            ["Statistics_SightingsByMunicipality"] = "Top Municipalities by Sightings",
            ["Statistics_TopSpecies"] = "Most Observed Species",
            ["Statistics_Sightings"] = "Sightings",
            ["Statistics_NoData"] = "No data available yet.",

            // Get Started Page
            ["PageTitle_GetStarted"] = "Get Started - FaunaFinder",
            ["Nav_GetStarted"] = "Get Started",
            ["GetStarted_WelcomeTitle"] = "Welcome to FaunaFinder",
            ["GetStarted_WelcomeSubtitle"] = "Discover and Document Puerto Rico's Wildlife",
            ["GetStarted_WelcomeDescription"] = "FaunaFinder is your gateway to exploring the rich biodiversity of Puerto Rico. Document wildlife sightings, explore species across the island, and join a community of students and researchers dedicated to conservation.",
            ["GetStarted_MissionTitle"] = "Our Mission",
            ["GetStarted_MissionDescription1"] = "FaunaFinder empowers students to become citizen scientists by documenting wildlife sightings across Puerto Rico. Every observation you make contributes valuable data to our understanding of local ecosystems and biodiversity.",
            ["GetStarted_MissionDescription2"] = "Together, we're building a comprehensive database of species locations across all 78 municipalities. Your contributions directly support conservation efforts, scientific research, and environmental education throughout the island.",
            ["GetStarted_StudentTitle"] = "Built for Students",
            ["GetStarted_StudentSubtitle"] = "Become a citizen scientist and make a real impact on conservation",
            ["GetStarted_StudentFeature1"] = "Document Wildlife",
            ["GetStarted_StudentFeature1_Desc"] = "Capture photos and GPS coordinates of species you discover",
            ["GetStarted_StudentFeature2"] = "Discover Biodiversity",
            ["GetStarted_StudentFeature2_Desc"] = "Explore hundreds of species across all 78 municipalities",
            ["GetStarted_StudentFeature3"] = "Learn & Contribute",
            ["GetStarted_StudentFeature3_Desc"] = "Understand conservation practices while building real data",
            ["GetStarted_StudentFeature4"] = "See Your Impact",
            ["GetStarted_StudentFeature4_Desc"] = "Track your observations and watch your contributions grow",
            ["GetStarted_NewHereTitle"] = "New Here?",
            ["GetStarted_NewHereDescription"] = "Request access to start contributing wildlife sightings and join our community of observers.",
            ["GetStarted_RequestAccess"] = "Request Access",
            ["GetStarted_HaveAccountTitle"] = "Have an Account?",
            ["GetStarted_HaveAccountDescription"] = "Sign in to continue reporting sightings and exploring wildlife data.",
            ["GetStarted_Login"] = "Sign In",
            ["GetStarted_Next"] = "Next",
        };

    public static IReadOnlyDictionary<string, string> Spanish { get; } =
        new Dictionary<string, string>
        {
            // Navigation
            ["Nav_Map"] = "Mapa",
            ["Nav_Species"] = "Especies",
            ["Nav_Pueblos"] = "Pueblos",
            ["Nav_About"] = "Acerca de",
            ["Nav_Sightings"] = "Avistamientos",
            ["Nav_Dashboard"] = "Panel",

            // Common
            ["AppTitle"] = "FaunaFinder",
            ["Loading"] = "Cargando...",
            ["LoadMore"] = "Cargar más",
            ["ShowLess"] = "Mostrar menos",
            ["Back"] = "Volver",
            ["TryAgain"] = "Intentar de nuevo",
            ["Close"] = "Cerrar",
            ["ViewDetails"] = "Ver detalles",
            ["ViewAllSpecies"] = "Ver todas las especies",
            ["ViewAll"] = "Ver todo",
            ["AllSpecies"] = "Todas las especies",
            ["Details"] = "Detalles",

            // Errors
            ["Error_SomethingWentWrong"] = "Algo salió mal",
            ["Error_UnexpectedError"] =
                "Ocurrió un error inesperado. Por favor, inténtelo de nuevo.",
            ["Error_SpeciesNotFound"] = "Especie no encontrada.",
            ["Error_MunicipalityNotFound"] = "Municipio no encontrado.",
            ["MunicipalityNotFound"] = "Municipio '{0}' no encontrado en la base de datos.",

            // Map Page
            ["Map_Loading"] = "Cargando mapa...",
            ["Map_ClickMunicipality"] =
                "Haz clic en un municipio en el mapa para ver información sobre especies y conservación.",
            ["Map_NoSpeciesData"] = "No hay datos de especies disponibles para este municipio.",
            ["Map_SpeciesFound"] = "{0} especies encontradas",
            ["Map_SpeciesInDatabase"] = "{0} especies en la base de datos",
            ["Map_ConservationLinks"] = "Enlaces de conservación",
            ["Map_ClearLocations"] = "Cerrar",
            ["Map_LocationsFound"] = "{0} ubicación(es) encontrada(s)",
            ["Map_ViewAllLocations"] = "Ver todas las ubicaciones",
            ["Map_BackToSpecies"] = "Volver a detalles de especie",
            ["Map_UnnamedLocation"] = "Ubicación",

            // Species Page
            ["Species_Title"] = "Especies",
            ["Species_Description"] =
                "Explora las especies de Puerto Rico y descubre dónde se pueden encontrar.",
            ["Species_SearchPlaceholder"] = "Buscar especies...",
            ["Species_NoResults"] = "No se encontraron especies que coincidan con tu búsqueda.",
            ["Species_Municipality"] = "municipio",
            ["Species_Municipalities"] = "municipios",
            ["Species_Showing"] = "Mostrando {0}-{1} de {2} especies",

            // Conservation Status
            ["Conservation_CriticallyImperiled"] = "En Peligro Crítico",
            ["Conservation_Imperiled"] = "En Peligro",
            ["Conservation_Vulnerable"] = "Vulnerable",
            ["Conservation_ApparentlySecure"] = "Aparentemente Segura",
            ["Conservation_Secure"] = "Segura",

            // Stats Hero
            ["Stats_Species"] = "Especies",
            ["Stats_Municipalities"] = "Municipios",
            ["Stats_Sightings"] = "Avistamientos",

            // Species Detail Page
            ["SpeciesDetail_FoundIn"] = "Encontrada en {0} {1}",
            ["SpeciesDetail_MunicipalitiesTitle"] = "Municipios",
            ["SpeciesDetail_NoMunicipalityData"] =
                "No hay datos de municipios disponibles para esta especie.",
            ["SpeciesDetail_ConservationLinksTitle"] = "Enlaces de conservación",
            ["SpeciesDetail_NoConservationLinks"] =
                "No hay enlaces de conservación disponibles para esta especie.",
            ["SpeciesDetail_ViewLocations"] = "Ver ubicaciones",
            ["SpeciesDetail_ImageSource"] = "Fuente de imagen",

            // Pueblos Page
            ["Pueblos_Title"] = "Pueblos de Puerto Rico",
            ["Pueblos_Description"] =
                "Explora los municipios de Puerto Rico y descubre su biodiversidad.",
            ["Pueblos_SearchPlaceholder"] = "Buscar municipios...",
            ["Pueblos_NoResults"] = "No se encontraron municipios que coincidan con tu búsqueda.",
            ["Pueblos_Species"] = "especies",
            ["Pueblos_Showing"] = "Mostrando {0}-{1} de {2} municipios",

            // Pueblo Detail Page
            ["PuebloDetail_SpeciesInMunicipality"] = "Especies en este municipio",
            ["PuebloDetail_NoSpeciesData"] =
                "No hay datos de especies disponibles para este municipio.",
            ["PuebloDetail_NoConservationLinks"] =
                "No hay enlaces de conservación disponibles para esta especie.",
            ["PuebloDetail_ViewLocation"] = "Ver en Mapa",

            // About Page
            ["About_Title"] = "Acerca de FaunaFinder",
            ["About_WhatIsTitle"] = "¿Qué es FaunaFinder?",
            ["About_WhatIsDescription"] =
                "FaunaFinder es una aplicación web interactiva que ayuda a los usuarios a explorar información de conservación para los municipios de Puerto Rico. Haz clic en cualquier municipio en el mapa para descubrir las especies que habitan en esa región, junto con las prácticas de conservación del NRCS y las recomendaciones de acción del FWS.",
            ["About_DataSourcesTitle"] = "Fuentes de datos",
            ["About_NrcsPractices"] = "Prácticas NRCS:",
            ["About_NrcsPracticesDesc"] =
                "Estándares de prácticas de conservación del Servicio de Conservación de Recursos Naturales",
            ["About_FwsActions"] = "Acciones FWS:",
            ["About_FwsActionsDesc"] =
                "Acciones de conservación recomendadas por el Servicio de Pesca y Vida Silvestre de EE.UU.",
            ["About_SpeciesData"] = "Datos de especies:",
            ["About_SpeciesDataDesc"] =
                "Información sobre ocurrencia y hábitat de especies en Puerto Rico",
            ["About_VisitNrcs"] = "Visitar Estándares de Prácticas NRCS",
            ["About_VisitEcos"] = "Visitar Perfiles de Especies ECOS",
            ["About_VisitFwsCaribbean"] = "Visitar FWS Caribe",
            ["About_AcknowledgmentsTitle"] = "Agradecimientos",
            ["About_AcknowledgmentsDesc"] =
                "FaunaFinder fue construido utilizando datos de conservación disponibles públicamente y tecnologías de código abierto.",
            ["About_SpeciesImages"] = "Imágenes de Especies:",
            ["About_SpeciesImagesDesc"] =
                "Las imágenes de perfil provienen de varias fuentes de dominio público y Creative Commons, con atribución mostrada en cada página de detalle de especies.",

            // Filter and Sort
            ["Filter_Sort"] = "Ordenar",
            ["Filter_NameAZ"] = "Nombre (A-Z)",
            ["Filter_NameZA"] = "Nombre (Z-A)",
            ["Filter_ScientificAZ"] = "Científico (A-Z)",
            ["Filter_ScientificZA"] = "Científico (Z-A)",
            ["Filter_Filters"] = "Filtros",
            ["Filter_ClearAll"] = "Limpiar",
            ["Filter_Apply"] = "Aplicar",
            ["Filter_NrcsPractices"] = "Prácticas NRCS",
            ["Filter_FwsActions"] = "Acciones FWS",
            ["Filter_ShowingFiltered"] = "Mostrando {0} de {1} especies",
            ["Filter_ShowingFilteredLinks"] = "Mostrando {0} de {1} enlaces",
            ["Filter_NoMatches"] = "Ninguna especie coincide con los filtros seleccionados.",
            ["Filter_NoMatchesLinks"] =
                "Ningun enlace de conservacion coincide con los filtros seleccionados.",
            ["Filter_SearchLinks"] = "Buscar enlaces de conservacion...",

            // Page Titles
            ["PageTitle_Map"] = "FaunaFinder - Puerto Rico",
            ["PageTitle_Species"] = "Especies - FaunaFinder",
            ["PageTitle_Pueblos"] = "Pueblos - FaunaFinder",
            ["PageTitle_About"] = "Acerca de - FaunaFinder",

            // Species Near Me
            ["NearMe_Title"] = "Especies cercanas",
            ["NearMe_Button"] = "Cercanas",
            ["NearMe_SelectRadius"] = "Seleccionar radio de busqueda",
            ["NearMe_Searching"] = "Buscando especies...",
            ["NearMe_NoSpeciesFound"] =
                "No se encontraron especies dentro de este radio. Intenta ampliar el area de busqueda.",
            ["NearMe_SpeciesFound"] = "{0} especies encontradas dentro de {1}km",
            ["NearMe_UseLocateFirst"] =
                "Usa el boton de ubicacion en el mapa para habilitar la busqueda de especies cerca de tu ubicacion.",
            ["NearMe_ShowLocations"] = "Mostrar Ubicaciones",
            ["NearMe_HideLocations"] = "Ocultar Ubicaciones",

            // Draw Polygon Search
            ["DrawPolygon_Title"] = "Area personalizada",
            ["DrawPolygon_Button"] = "Dibujar forma",
            ["DrawPolygon_Tooltip"] = "Dibuja una forma personalizada en el mapa para buscar especies",
            ["DrawPolygon_Instructions"] = "Haz clic para agregar puntos",
            ["DrawPolygon_Redraw"] = "Redibujar forma",
            ["DrawPolygon_Finish"] = "Terminar",
            ["DrawPolygon_SpeciesFound"] = "{0} especies encontradas en el area seleccionada",

            // Heatmap
            ["Heatmap_Toggle"] = "Mapa de calor",
            ["Heatmap_Tooltip"] = "Mostrar mapa de calor de densidad de especies",
            ["Heatmap_Loading"] = "Cargando datos del mapa de calor...",
            ["Heatmap_FilterAll"] = "Todas",
            ["Heatmap_FilterFauna"] = "Fauna",
            ["Heatmap_FilterFlora"] = "Flora",

            // Export
            ["Export_Button"] = "Exportar",
            ["Export_PDF"] = "Descargar PDF",
            ["Export_CSV"] = "Descargar CSV",
            ["Export_Generating"] = "Generando informe...",

            // Login
            ["Login_Title"] = "Iniciar sesión",
            ["Login_Description"] = "Inicia sesión para acceder a las funciones de contribuidor.",
            ["Login_SignIn"] = "Iniciar sesión",
            ["Login_Email"] = "Correo electrónico",
            ["Login_Password"] = "Contraseña",
            ["Login_RememberMe"] = "Recordarme",
            ["Login_EmailRequired"] = "El correo electrónico es requerido",
            ["Login_PasswordRequired"] = "La contraseña es requerida",
            ["Login_AlreadyLoggedIn"] = "Has iniciado sesión como {0}",
            ["Login_GoToHome"] = "Ir al inicio",
            ["Login_SignOut"] = "Cerrar sesión",
            ["Login_Profile"] = "Perfil",
            ["Login_PendingApproval"] = "Tu cuenta está pendiente de aprobación",
            ["Login_NoAccount"] = "¿No tienes cuenta? ",
            ["Login_CreateAccount"] = "Crear una",

            // Register
            ["Register_Title"] = "Crear cuenta",
            ["Register_Description"] = "Crea una cuenta para contribuir a FaunaFinder.",
            ["Register_DisplayName"] = "Nombre",
            ["Register_DisplayNameRequired"] = "El nombre es requerido",
            ["Register_ConfirmPassword"] = "Confirmar contraseña",
            ["Register_ConfirmPasswordRequired"] = "Por favor confirma tu contraseña",
            ["Register_PasswordRequirements"] =
                "Al menos 8 caracteres con mayúscula, minúscula y número",
            ["Register_PasswordMismatch"] = "Las contraseñas no coinciden",
            ["Register_CreateAccount"] = "Crear cuenta",
            ["Register_HasAccount"] = "¿Ya tienes cuenta? ",
            ["Register_Success"] =
                "Cuenta creada exitosamente. Tu cuenta está pendiente de aprobación.",
            ["Register_Message"] = "Mensaje (opcional)",
            ["Register_MessageHelper"] = "Cuéntanos por qué quieres acceso",
            ["Register_SuccessPending"] =
                "Registro enviado! Un administrador revisará tu solicitud.",

            // Admin
            ["Admin_Title"] = "Gestión de usuarios",
            ["Admin_Description"] = "Administra cuentas y permisos de usuarios.",
            ["Admin_NoUsers"] = "No se encontraron usuarios.",
            ["Admin_Email"] = "Correo electrónico",
            ["Admin_DisplayName"] = "Nombre",
            ["Admin_Status"] = "Estado",
            ["Admin_Role"] = "Rol",
            ["Admin_CreatedAt"] = "Creado",
            ["Admin_Actions"] = "Acciones",
            ["Admin_Approve"] = "Aprobar",
            ["Admin_Reject"] = "Rechazar",
            ["Admin_MakeAdmin"] = "Hacer Admin",
            ["Admin_MakeContributor"] = "Hacer Contribuidor",
            ["Admin_MakeViewer"] = "Hacer Lector",
            ["Admin_Pending"] = "Pendiente",
            ["Admin_Approved"] = "Aprobado",
            ["Admin_Rejected"] = "Rechazado",
            ["Admin_Viewer"] = "Lector",
            ["Admin_Contributor"] = "Contribuidor",
            ["Admin_Admin"] = "Admin",
            ["Admin_AccessDenied"] = "Acceso denegado",
            ["Admin_AccessDeniedMessage"] = "No tienes permiso para acceder a esta página.",
            ["Admin_ViewDetails"] = "Ver Detalles",
            ["Admin_UserNotFound"] = "Usuario no encontrado.",
            ["Admin_UserInfo"] = "Información del Usuario",
            ["Admin_PendingActionDescription"] =
                "Este usuario está esperando aprobación. Aprueba o rechaza su solicitud de cuenta.",
            ["Admin_NoActionsAvailable"] = "No hay acciones disponibles para este usuario.",
            ["Admin_AccessRequests"] = "Solicitudes de Acceso",
            ["Admin_AccessRequestDetails"] = "Detalles de Solicitud",
            ["Admin_Message"] = "Mensaje",
            ["Admin_NoAccessRequests"] = "No hay solicitudes de acceso pendientes.",
            ["Admin_AccessRequests_Title"] = "Solicitudes de Acceso",
            ["Admin_AccessRequests_Description"] =
                "Revisar y gestionar solicitudes de registro de usuarios.",
            ["Admin_AccessRequests_SearchPlaceholder"] = "Buscar por nombre o correo...",
            ["Admin_AccessRequests_StatusFilter"] = "Estado",
            ["Admin_AccessRequests_StatusAll"] = "Todos",
            ["Nav_Admin"] = "Admin",

            // User Menu
            ["UserMenu_Logout"] = "Cerrar sesión",

            // Wildlife Sighting
            ["Sighting_ReportTitle"] = "Reportar Avistamiento",
            ["Sighting_Mode"] = "Modo de Avistamiento",
            ["Sighting_ModeCasual"] = "Casual - Avistamiento ocasional",
            ["Sighting_ModeSurvey"] = "Encuesta - Búsqueda sistemática",
            ["Sighting_Species"] = "Especie",
            ["Sighting_SearchSpecies"] = "Buscar Especie",
            ["Sighting_SearchPlaceholder"] = "Comienza a escribir para buscar...",
            ["Sighting_SpeciesRequired"] = "La especie es requerida",
            ["Sighting_ObservationDetails"] = "Detalles de la Observación",
            ["Sighting_Confidence"] = "Nivel de Confianza",
            ["Sighting_ConfidenceCertain"] = "Seguro - Definitivamente identificado",
            ["Sighting_ConfidenceFairlySure"] = "Bastante seguro - Probablemente correcto",
            ["Sighting_ConfidenceUnsure"] = "Inseguro - Mejor suposición",
            ["Sighting_Count"] = "Estimación de Cantidad",
            ["Sighting_Behavior"] = "Comportamiento Observado",
            ["Sighting_BehaviorFeeding"] = "Alimentándose",
            ["Sighting_BehaviorResting"] = "Descansando",
            ["Sighting_BehaviorMoving"] = "Moviéndose",
            ["Sighting_BehaviorCalling"] = "Vocalizando",
            ["Sighting_Evidence"] = "Tipo de Evidencia",
            ["Sighting_EvidenceVisual"] = "Visual",
            ["Sighting_EvidenceHeard"] = "Escuchado",
            ["Sighting_EvidenceTracks"] = "Huellas/Señales",
            ["Sighting_EvidencePhoto"] = "Foto",
            ["Sighting_EvidenceRequired"] = "Se requiere al menos un tipo de evidencia",
            ["Sighting_Weather"] = "Clima (opcional)",
            ["Sighting_WeatherClear"] = "Despejado",
            ["Sighting_WeatherPartlyCloudy"] = "Parcialmente Nublado",
            ["Sighting_WeatherCloudy"] = "Nublado",
            ["Sighting_WeatherRainy"] = "Lluvioso",
            ["Sighting_WeatherStormy"] = "Tormentoso",
            ["Sighting_WeatherFoggy"] = "Neblinoso",
            ["Sighting_WeatherWindy"] = "Ventoso",
            ["Sighting_Notes"] = "Notas (opcional)",
            ["Sighting_Location"] = "Ubicación",
            ["Sighting_Latitude"] = "Latitud",
            ["Sighting_Longitude"] = "Longitud",
            ["Sighting_GetLocation"] = "Obtener Ubicación Actual",
            ["Sighting_ObservationTime"] = "Hora de Observación",
            ["Sighting_Date"] = "Fecha",
            ["Sighting_Time"] = "Hora",
            ["Sighting_FutureDateError"] =
                "La fecha y hora de observación no pueden ser en el futuro",
            ["Sighting_Photo"] = "Foto (opcional)",
            ["Sighting_UploadPhoto"] = "Subir Foto",
            ["Sighting_Submit"] = "Enviar Reporte de Avistamiento",
            ["Sighting_Unauthorized"] = "Debes iniciar sesión para enviar un avistamiento",
            ["Sighting_UnexpectedError"] =
                "Ocurrió un error inesperado. Por favor, inténtelo de nuevo.",

            // My Sightings
            ["Sightings_MyTitle"] = "Mis Avistamientos",
            ["Sightings_Report"] = "Reportar",
            ["Sightings_NoSightings"] = "No hay avistamientos",
            ["Sightings_NoSightingsDescription"] =
                "Aún no has reportado ningún avistamiento de fauna.",
            ["Sightings_ReportFirst"] = "Reportar Tu Primer Avistamiento",
            ["Sightings_UnknownSpecies"] = "Especie Desconocida",
            ["Sightings_Unauthorized"] = "Debes iniciar sesión para ver tus avistamientos",
            ["Sightings_StatusApproved"] = "Aprobado",
            ["Sightings_StatusRejected"] = "Rechazado",
            ["Sightings_StatusPending"] = "Pendiente",

            // Sighting Detail
            ["SightingDetail_Title"] = "Detalles del Avistamiento",
            ["SightingDetail_NotFound"] = "Avistamiento no encontrado.",
            ["SightingDetail_Photo"] = "Foto",
            ["SightingDetail_PhotoAlt"] = "Foto del avistamiento",
            ["SightingDetail_NoPhoto"] = "No hay foto disponible",
            ["SightingDetail_Location"] = "Ubicación",
            ["SightingDetail_ObservationDetails"] = "Detalles de la Observación",
            ["SightingDetail_ObservedAt"] = "Observado",
            ["SightingDetail_ReportedAt"] = "Reportado",
            ["SightingDetail_Weather"] = "Clima",
            ["SightingDetail_Notes"] = "Notas",
            ["SightingDetail_ReviewStatus"] = "Estado de Revisión",
            ["SightingDetail_ReviewedAt"] = "Revisado",
            ["SightingDetail_ReviewNotes"] = "Notas de Revisión",
            ["SightingDetail_FlaggedForReview"] =
                "Este avistamiento ha sido marcado para revisión de expertos.",
            ["SightingDetail_NewMunicipalityRecord"] =
                "¡Este es un nuevo registro de especie para este municipio!",
            ["SightingDetail_PendingReviewMessage"] =
                "Tu avistamiento está esperando revisión por un maestro o administrador.",

            // Sighting Detail Photo Upload
            ["SightingDetail_AddPhoto"] = "Agregar Foto",
            ["SightingDetail_ChangePhoto"] = "Cambiar Foto",
            ["SightingDetail_UploadingPhoto"] = "Subiendo...",
            ["SightingDetail_PhotoUploaded"] = "Foto subida exitosamente",
            ["SightingDetail_PhotoUploadError"] =
                "Error al subir la foto. Por favor, inténtelo de nuevo.",
            ["SightingDetail_InvalidFileType"] =
                "Tipo de archivo no válido. Solo se permiten imágenes JPEG, PNG, GIF y WebP.",
            ["SightingDetail_FileTooLarge"] =
                "El archivo es demasiado grande. El tamaño máximo es 5MB.",

            // Dashboard
            ["Dashboard_Title"] = "Panel",
            ["Dashboard_ReviewTab"] = "Revisar",
            ["Dashboard_AdminPanel_Title"] = "Administracion",
            ["Dashboard_AdminPanel_Description"] =
                "Gestiona usuarios, revisa solicitudes y supervisa la actividad de la plataforma.",
            ["Dashboard_Card_UserManagement"] = "Gestion de Usuarios",
            ["Dashboard_Card_UserManagement_Desc"] =
                "Ver, buscar y gestionar todas las cuentas de usuario.",
            ["Dashboard_Card_UserRequests"] = "Solicitudes de Usuarios",
            ["Dashboard_Card_UserRequests_Desc"] =
                "Revisar y aprobar solicitudes de registro pendientes.",
            ["Dashboard_Card_UsersSightings"] = "Avistamientos de Usuarios",
            ["Dashboard_Card_UsersSightings_Desc"] =
                "Ver y gestionar avistamientos reportados por usuarios.",

            // Admin Users
            ["Admin_Users_SearchPlaceholder"] = "Buscar por nombre o correo...",

            // Review Queue
            ["ReviewQueue_Title"] = "Cola de Revisión",
            ["ReviewQueue_Empty"] = "No hay avistamientos pendientes de revisión",
            ["ReviewQueue_EmptyDescription"] =
                "Todos los avistamientos han sido revisados. Vuelve más tarde para ver nuevas presentaciones.",
            ["ReviewQueue_Approve"] = "Aprobar",
            ["ReviewQueue_Reject"] = "Rechazar",
            ["ReviewQueue_ReviewNotes"] = "Notas de Revisión (opcional)",
            ["ReviewQueue_ReviewNotesPlaceholder"] = "Agregar notas sobre esta revisión...",
            ["ReviewQueue_ApproveSuccess"] = "Avistamiento aprobado exitosamente",
            ["ReviewQueue_RejectSuccess"] = "Avistamiento rechazado exitosamente",
            ["ReviewQueue_ReviewError"] =
                "Error al revisar el avistamiento. Por favor, inténtelo de nuevo.",
            ["ReviewQueue_ObservedOn"] = "Observado",
            ["ReviewQueue_ReportedOn"] = "Reportado",
            ["ReviewQueue_Location"] = "Ubicación",
            ["ReviewQueue_ViewDetails"] = "Ver Detalles",
            ["ReviewQueue_CloseDetails"] = "Cerrar",
            ["ReviewQueue_Unauthorized"] = "No tienes permiso para acceder a esta página.",
            ["Nav_ReviewQueue"] = "Cola de Revisión",

            // Statistics
            ["Nav_Statistics"] = "Estadísticas",
            ["PageTitle_Statistics"] = "Estadísticas - FaunaFinder",
            ["Statistics_Title"] = "Estadísticas de Vida Silvestre",
            ["Statistics_Description"] = "Explora datos agregados y tendencias de avistamientos de vida silvestre en Puerto Rico.",
            ["Statistics_TotalSightings"] = "Total de Avistamientos",
            ["Statistics_TotalSpecies"] = "Especies en Base de Datos",
            ["Statistics_TotalMunicipalities"] = "Municipios",
            ["Statistics_ThisMonth"] = "Avistamientos Este Mes",
            ["Statistics_SightingsOverTime"] = "Avistamientos a lo Largo del Tiempo",
            ["Statistics_SpeciesByCategory"] = "Especies por Categoría",
            ["Statistics_SightingsByMunicipality"] = "Principales Municipios por Avistamientos",
            ["Statistics_TopSpecies"] = "Especies Más Observadas",
            ["Statistics_Sightings"] = "Avistamientos",
            ["Statistics_NoData"] = "No hay datos disponibles aún.",

            // Get Started Page
            ["PageTitle_GetStarted"] = "Comenzar - FaunaFinder",
            ["Nav_GetStarted"] = "Comenzar",
            ["GetStarted_WelcomeTitle"] = "Bienvenido a FaunaFinder",
            ["GetStarted_WelcomeSubtitle"] = "Descubre y Documenta la Vida Silvestre de Puerto Rico",
            ["GetStarted_WelcomeDescription"] = "FaunaFinder es tu puerta de entrada para explorar la rica biodiversidad de Puerto Rico. Documenta avistamientos de vida silvestre, explora especies en toda la isla y únete a una comunidad de estudiantes e investigadores dedicados a la conservación.",
            ["GetStarted_MissionTitle"] = "Nuestra Misión",
            ["GetStarted_MissionDescription1"] = "FaunaFinder empodera a los estudiantes para convertirse en científicos ciudadanos documentando avistamientos de vida silvestre en todo Puerto Rico. Cada observación que realices aporta datos valiosos para comprender nuestros ecosistemas locales y su biodiversidad.",
            ["GetStarted_MissionDescription2"] = "Juntos, estamos construyendo una base de datos completa de ubicaciones de especies en los 78 municipios. Tus contribuciones apoyan directamente los esfuerzos de conservación, la investigación científica y la educación ambiental en toda la isla.",
            ["GetStarted_StudentTitle"] = "Diseñado para Estudiantes",
            ["GetStarted_StudentSubtitle"] = "Conviértete en científico ciudadano y genera un impacto real en la conservación",
            ["GetStarted_StudentFeature1"] = "Documenta Vida Silvestre",
            ["GetStarted_StudentFeature1_Desc"] = "Captura fotos y coordenadas GPS de las especies que descubras",
            ["GetStarted_StudentFeature2"] = "Descubre Biodiversidad",
            ["GetStarted_StudentFeature2_Desc"] = "Explora cientos de especies en los 78 municipios",
            ["GetStarted_StudentFeature3"] = "Aprende y Contribuye",
            ["GetStarted_StudentFeature3_Desc"] = "Comprende las prácticas de conservación mientras generas datos reales",
            ["GetStarted_StudentFeature4"] = "Mira Tu Impacto",
            ["GetStarted_StudentFeature4_Desc"] = "Rastrea tus observaciones y mira crecer tus contribuciones",
            ["GetStarted_NewHereTitle"] = "¿Eres Nuevo?",
            ["GetStarted_NewHereDescription"] = "Solicita acceso para comenzar a contribuir avistamientos de vida silvestre y únete a nuestra comunidad de observadores.",
            ["GetStarted_RequestAccess"] = "Solicitar Acceso",
            ["GetStarted_HaveAccountTitle"] = "¿Tienes una Cuenta?",
            ["GetStarted_HaveAccountDescription"] = "Inicia sesión para continuar reportando avistamientos y explorando datos de vida silvestre.",
            ["GetStarted_Login"] = "Iniciar Sesión",
            ["GetStarted_Next"] = "Siguiente",
        };
}
