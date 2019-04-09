#!/usr/bin/env bash

set -e
set -x
mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/
set +x

if [[ "${UNITY_LICENSE_CONTENT:-}" ]]
then
    echo "Writing \$UNITY_LICENSE_CONTENT to license file /root/.local/share/unity3d/Unity/Unity_lic.ulf"
    echo "$UNITY_LICENSE_CONTENT" | tr -d '\r' > /root/.local/share/unity3d/Unity/Unity_lic.ulf
elif [[ "${UNITY_LICENSE_CONTENT_B64:-}" ]]
then
    echo "Writing \$UNITY_LICENSE_CONTENT_B64 to license file /root/.local/share/unity3d/Unity/Unity_lic.ulf"
    UNITY_LICENSE_CONTENT="$(base64 -di - <<< "$UNITY_LICENSE_CONTENT_B64")"
    echo "$UNITY_LICENSE_CONTENT" | tr -d '\r' > /root/.local/share/unity3d/Unity/Unity_lic.ulf
else
    echo "Missing \$UNITY_LICENSE_CONTENT variable. Aborting"
    exit 1
fi

# ${UNITY_EXECUTABLE:-xvfb-run -as '-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
#     -batchmode \
#     -username "$UNITY_USERNAME" \
#     -password "$UNITY_PASSWORD" \
#     -quit \
#     -logfile

exit
