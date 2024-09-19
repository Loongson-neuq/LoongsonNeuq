# /bin/bash

export DEPLOY_FOLDER="publish"
export MAIN_EXE="LoongsonNeuq.AssignmentSubmit"
export PACK_ID="AssignmentSubmit"
export PACK_VERSION="1.0.0"
export OUTPUT_DIR="output"

echo "Cleaning up previous build"
rm -rf $DEPLOY_FOLDER

echo "Building and deploying to $DEPLOY_FOLDER"
dotnet publish -c Release -r linux-x64 -p:PublishAot=true -o $DEPLOY_FOLDER

echo "Cleaning debug symbols and unnecessary files"
rm -rf $DEPLOY_FOLDER/*.pdb
rm -rf $DEPLOY_FOLDER/*.dbg
rm -rf $DEPLOY_FOLDER/*.json
rm -rf $DEPLOY_FOLDER/LoongsonNeuq.AutoGrader

echo "Publishing .AppImage with Velopack"
vpk pack \
    --mainExe $MAIN_EXE \
    --packDir $DEPLOY_FOLDER \
    --packId $PACK_ID \
    --packVersion $PACK_VERSION \
    --outputDir $OUTPUT_DIR