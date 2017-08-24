# 参考
# https://jonathan.porta.codes/2015/04/17/automatically-build-your-unity3d-project-in-the-cloud-using-travisci-for-free/

#! /bin/sh

UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"

echo "Run Unity Test Runner"

${UNITY_PATH} \
	-runEditorTests \
	-projectPath ${pwd} \
	-logFile ${pwd}/testrunner.log \
	-editorTestsVerboseLog \
	-batchmode \
	-nographics \
	-quit