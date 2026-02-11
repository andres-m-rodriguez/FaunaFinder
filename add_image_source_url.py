"""
Add imageSourceUrl field to species.json for entries that have imageBase64.
The URL is the Wikipedia article for the species' scientific name.
"""
import json
from collections import OrderedDict

FILE_PATH = r"C:\Users\Overlord\Downloads\FaunaFinder\src\FaunaFinder.Seeder\Data\species.json"

with open(FILE_PATH, "r", encoding="utf-8") as f:
    data = json.load(f, object_pairs_hook=OrderedDict)

modified_count = 0

for entry in data:
    if entry.get("imageBase64"):
        scientific_name = entry["scientificName"]
        wiki_name = scientific_name.replace(" ", "_")
        url = f"https://en.wikipedia.org/wiki/{wiki_name}"

        # Rebuild the OrderedDict with imageSourceUrl after imageContentType
        new_entry = OrderedDict()
        for key, value in entry.items():
            new_entry[key] = value
            if key == "imageContentType":
                new_entry["imageSourceUrl"] = url

        # Replace the entry contents
        entry.clear()
        entry.update(new_entry)
        modified_count += 1

with open(FILE_PATH, "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
    f.write("\n")

print(f"Modified {modified_count} entries with imageSourceUrl.")
