# Currently not working - for some reason NuGet doesn't like the packages I create. I'm working on it.


# iResign
## What to do when $BIGCOMPANY enforces AllSigned via Group Policy
This application will sign the ps1 files inside NuGet packages with a codesigning certificate, then repack them for use in a local package cache.

# Codesigning certificates
If you don't have a codesigning certificate, create one via:
```
makecert -n "CN=PowerShell Local Certificate Root" -a sha1 -eku 1.3.6.1.5.5.7.3.3 -r -sv root.pvk root.cer -ss Root -sr localMachine
makecert -pe -n "CN=PowerShell User" -ss MY -a sha1 -eku 1.3.6.1.5.5.7.3.3 -iv root.pvk -ic root.cer
```

(http://stackoverflow.com/a/18583725)


# To Do
* This is definitely just a hack right now, the output paths are hard coded.
* Needs command line options, more verbose output, ability to skip packages that are already signed by trusted certs etc
