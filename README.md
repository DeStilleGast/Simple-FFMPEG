# Simple-FFMPEG
Simple GUI for FFMPEG, primary usage goal was for Discord


# Usage
Download the zip file that comes bundles with or without ffmpeg, extract the files somewhere and execute `Wpf simple FFMPEG.exe`

if you chose to download without ffmpeg bundles, you have to download it your self from [here](https://ffmpeg.org/download.html)
extract `ffmpeg.exe` and `ffprobe.exe`, place it in the directory where you have placed the exe file (it can also be in a sub directory `bin` or `tools`)


![5F1SeOdWXc](https://user-images.githubusercontent.com/3677706/165175272-15bb55eb-0003-46cb-ba05-f014b9434841.png)

You can drag & drop files in the input textbox and output textbox, you can also use the button next to it with content `...` this will open a file select dialog.
the folder button next to the output can predefine the default output directory to save the 'new compressed' file, you can also remove the predefined directory here.

### Video trimming
for `video trimming` to work you need `ffprobe.exe` in the given folders explained above (technical: ffprobe is the specific tool to extract information from video files, currently it extract the video length, video dimension and the framerate (fps))

### Rescale
by default I have included `1280x720` and `320x280`, in the cog icon button you can declare your own scaling options

### Blur area's
Here you can declare area's that you want to be blurred, you can add so many as you want but this might delay/slow down the encoding, tip: you can use the `delete` button to remove the selected area
![afbeelding](https://user-images.githubusercontent.com/3677706/165177374-fc84134d-0d64-4a5b-9199-c008cf7a4044.png)

### Modify FPS
Change the framerate of your video, reducing the FPS can reduce the final file size

### Video information
Just displays some video information, the length, dimensions and FPS

### Credits
button left on `Go` is the credits button, here I declared the used sources

### Go
execute FFMPEG with given parameters, a terminal screen will pop up and might ask if you want to override the exising file, when it is done this application will ask if you want to see the file in explorer
