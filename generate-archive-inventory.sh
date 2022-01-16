#!/usr/bin/env bash

set -e

ARCHIVE_PATH="$1"
DESTINATION_PATH="$2"

if [[ -z "$ARCHIVE_PATH" ]]; then
    echo "First parameter must be the full path the archive."
    exit 1
fi

if [[ -z "$DESTINATION_PATH" ]]; then
    echo "Second parameter must be the full output path of the inventory report."
    exit 1
fi

ARCHIVE_PATH=$(readlink -f "$ARCHIVE_PATH")
DESTINATION_PATH=$(readlink -f "$DESTINATION_PATH")

# create temp directory
DEST_DIR=$(dirname $(readlink -f "$0"))
TEMP_DIR=$(mktemp -d)

# setup auto cleanup
trap "rm -rf $TEMP_DIR" EXIT

pushd "$TEMP_DIR"
echo "Using temporary directory $TEMP_DIR"
echo "Target directory is $DEST_DIR"
echo "Input archive is $ARCHIVE_PATH"

TEMP_ARCHIVE_PATH="$TEMP_DIR/temp"
TEMP_INV_PATH="$TEMP_DIR/files.txt"
mkdir "$TEMP_ARCHIVE_PATH"

case "$ARCHIVE_PATH" in
  *.tar)
    tar -xf "$ARCHIVE_PATH" --directory "$TEMP_ARCHIVE_PATH"
    ;;

  *.tar.gz)
    tar -zxf "$ARCHIVE_PATH" --directory "$TEMP_ARCHIVE_PATH"
    ;;

  *)
    echo -n "Unsupported archive type ${ARCHIVE_PATH#*.}."
    exit 1
    ;;
esac

find "$TEMP_ARCHIVE_PATH" -type f -print0 | 
    while IFS= read -r -d '' FILE_PATH; do
        HASH=($(sha256sum "$FILE_PATH"))
        
        REL_PATH=$(realpath --relative-to="$TEMP_ARCHIVE_PATH" "$FILE_PATH")

        echo "$HASH;$REL_PATH" >> "$TEMP_INV_PATH"
    done

mv "$TEMP_INV_PATH" "$DESTINATION_PATH"
echo "Inventory file created at $DESTINATION_PATH"

popd # exit temp directory

echo "Done!"
