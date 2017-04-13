# TTSAutomate

## _Download the tool [here](https://github.com/CaffeineAU/TTSAutomate/releases/latest)_

A tool to generate Audio files from text strings in WAV and MP3 format, using various TTS engines as the source

Now supports multiple languages (if there's a language you'd like supported, and you can assist with translation, please let me know):

* Czech
* English
* French
* German
* Portuguese
* Slovak

# A quick usage guide:

* Select Open Phrase File (Ctrl-O) to open an existing psv file, or enter new phrases directly into the blank lines
* Select Output Directory (Ctrl-P) to choose where to save the audio files. The files will be saved under two directories under the selected folder; One for MP3, one for WAV, and under that each voice file will be saved with the Folder and Filenames specified
* Add, or modify the folder, filename or phrases in the list of phrases below.
* Select a TTS provider and voice using the drop down boxes.
* Preview a line to hear how that voice will sound.
* Select Go (Ctrl-G) to start the downloading process. As each file is downloaded, the preview button next to it changes to Play, which you can use to listen to the voice from within the tool. Select Stop (Ctrl-H) to stop the download process, or let it run to completion (the progress bar indicates overall progress)
* If you want to modify any of the phrases, edit the phrase in the list, and preview again to hear the change. You will note that the Play button changes to Preview after you edit the line. You can select Go (Ctrl-G) again to have the tool download only those missing files.
* If you have modified your phrases file, or created a new phrases file, you can save the file with the Save Phrases File (Ctrl-S) button, or save the file with a new name using the Save Phrases File As... (Ctrl-A) button.
* You can move lines up and down, by selecting the lines (click on the row header and using the buttons.
* You can add new rows above / below selected lines, or delete lines
* Subfolders in the Folders column are supported, for example: if you specify _voice\user_, then the files will be generated in _<output directory>\mp3\voice\user\_ and _<output directory>\wav\voice\user\_


**You will need [Microsoft .Net 4.5.2](https://www.microsoft.com/en-au/download/details.aspx?id=42643) to run this tool**
