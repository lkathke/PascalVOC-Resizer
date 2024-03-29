# PascalVOC-Resizer
A tool written in C#.NET to resize annotated images in PascalVOC format (with zero-pixel padding).
I developed this tool to match the 300x300 size of mobilenet ssd. This tool resizes (stretch to fit) images to a defined size (e.g. 300x300). Aspect ratio is not changed -> Gaps are filled with zeros


## Original image
![Original image](img/original.jpg)

## Resized image
![Resized image](img/resized.jpg)


# Libraries
This project is using
-EmguCV (https://github.com/commandlineparser/commandline)
-CommandLine (https://github.com/commandlineparser/commandline)

# How to use
## Example

This example uses a
- Image-directory (D:\Wagon-mit-Containern-PascalVOC-export\JPEGImages)
- XML-Annotation directory (D:\Wagon-mit-Containern-PascalVOC-export\Annotations)
- Output-directory (D:\wagon_mit_containern)
- Image width of 300 pixels
- root-directory name "wagons" (which is defined in the annotation xml file) -> this is optional!

```
ImageResizer.exe -i "D:\Wagon-mit-Containern-PascalVOC-export\JPEGImages" -x "D:\Wagon-mit-Containern-PascalVOC-export\Annotations" -o "D:\wagon_mit_containern" -w 300 -r wagons
```


This example uses a
- Image-directory with annotation xml-files (D:\Wagon-mit-Containern-PascalVOC-export\JPEGImages)
- Output-directory (D:\wagon_mit_containern)
- Image width of 300 pixels
- root-directory name "wagons" (which is defined in the annotation xml file) -> this is optional!

```
ImageResizer.exe -i "D:\Wagon-mit-Containern-PascalVOC-export\JPEGImages" -o "D:\wagon_mit_containern" -w 300 -r wagons
```

This example uses a
- Image-directory with annotation xml-files (D:\Wagon-mit-Containern-PascalVOC-export\JPEGImages)
- Output-directory (D:\wagon_mit_containern)
- Image width of 400 pixels
- Image height of 300 pixels
- root-directory name "wagons" (which is defined in the annotation xml file) -> this is optional!

```
ImageResizer.exe -i "D:\Wagon-mit-Containern-PascalVOC-export\JPEGImages" -o "D:\wagon_mit_containern" -w 400 -h 300 -r wagons
```

## Arguments:
You can see the arguments by running ImageResizer:

```
PS C:\Users\lkathke\Desktop\PascalVOC-Resizer> ImageResizer.exe
ImageResizer 1.0.0.0
Copyright ©  2019

ERROR(S):
  Required option 'i' is missing.
  Required option 'o' is missing.
  Required option 'w' is missing.

  -i           Required. The input directory (jpg and xml files are in there)

  -x           (Default: ) XML input directory (optional, if not set, using the same directory for images)

  -o           Required. The output directory

  -w           Required. The Image width

  -h           (Default: -1) The Image height

  -r           (Default: ) The root directory to write in the xml file

  --help       Display this help screen.

  --version    Display version information.
  ```
  
  # To-Do's
  - Images in portrait aspect ratio do not work (only landscape)

  
