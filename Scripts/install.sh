# 参考
# https://jonathan.porta.codes/2015/04/17/automatically-build-your-unity3d-project-in-the-cloud-using-travisci-for-free/

#! /bin/sh

HASH="892c0f8d8f8a"
VERSION="2017.1.0p4"
UNITY_EDITOR_URL="http://beta.unity3d.com/download/${HASH}/MacEditorInstaller/Unity-${VERSION}.pkg"

echo "Downloading from ${UNITY_EDITOR_URL}: "
curl -o Unity.pkg ${UNITY_EDITOR_URL}

echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /