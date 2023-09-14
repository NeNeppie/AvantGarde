import csv
import json

"""
This is a small script meant to add any new categories to the data.json file if a patch adds new ones.
The csv file is an exported sheet from Godbert

Side Note: This script is incredibly stupid and clearly very poorly written.
It gets the job done, but man, I slightly hate myself for this.
"""

categories = []

with open("FashionCheckThemeCategory.csv", 'r', encoding="utf-8") as file:
    csv_file = file.readlines()[1:]

csv_reader = csv.DictReader(csv_file)
for i, row in enumerate(csv_reader):
    if not row["#"].isnumeric() or not row["Name"]:
        continue
    categories.append(row["Name"])

with open("FashionReporter\\data.json", 'r', encoding="utf-8") as json_file:
    data = json.loads(json_file.read())

field_counter = 0
for category in categories:
    skip = False
    for x in data:
        if category == x['Name']:
            skip = True
    if not skip:
        data.insert(field_counter, ({'Name': category, 'IDs': []}))
    field_counter += 1
json_object = json.dumps(data, indent=4, ensure_ascii=False)

with open("FashionReporter\\data.json", 'w', encoding="utf-8") as json_file:
    json_file.write(json_object)
