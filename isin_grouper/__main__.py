import io
import json
import pandas as pd
from pathlib import Path
import sys


filename = Path(__file__).parent / "replacements.txt"
replacements = {}

try:
    with open(filename, "r", encoding="utf-8") as f:
        for line in f.readlines():
            key, value = line.strip().split("=>")
            replacements[key.strip()] = value.strip()
except UnicodeDecodeError:
    with open(filename, "r", encoding="cp1252") as f:
        for line in f.readlines():
            key, value = line.strip().split("=>")
            replacements[key.strip()] = value.strip()


class ISINGrouper:
    def __init__(self, folder_path, save_folder_path):
        self.folder_path = Path(folder_path)
        self.save_folder_path = Path(save_folder_path)
        self.columns = None

    def get_df(self, path, counter):
        def _get_content(path, encoding):
            with open(path, "r", encoding=encoding) as f:
                content = f.read()
                for key, value in replacements.items():
                    content = content.replace(key, value)
            return content

        print(
            json.dumps(
                {"type": "READ_FILE", "filename": path, "counter": counter}
            )
        )
        if path.lower().endswith(".csv") or path.lower().endswith(".cds"):
            try:
                # Read the file contents and perform replacements
                content = _get_content(path, encoding="utf-8")

                # Read the modified content as a pandas dataframe
                df = pd.read_csv(
                    io.StringIO(content),
                    header=None,
                    delimiter="~",
                    low_memory=False,
                    dtype=str,
                )
            except UnicodeDecodeError:
                content = _get_content(path, encoding="cp1252")

                df = pd.read_csv(
                    path,
                    header=None,
                    delimiter="~",
                    encoding="cp1252",
                    low_memory=False,
                    dtype=str,
                )
        elif path.lower().endswith(".xlsx") or path.lower().endswith(".xls"):
            df = pd.read_excel(path, header=None, low_memory=False, dtype=str)
        else:
            raise ValueError(
                f"Error: Unsupported file type {Path(path).suffix}"
            )
        return df

    def group_by_ISIN(self):
        file_paths = [
            *self.folder_path.glob("*.csv"),
            *self.folder_path.glob("*.CSV"),
            *self.folder_path.glob("*.cds"),
            *self.folder_path.glob("*.CDS"),
        ]
        print(json.dumps({"type": "TOTAL_READS", "counts": len(file_paths)}))
        file_paths = [str(path) for path in file_paths]

        df_complete = pd.concat(
            [
                self.get_df(path, counter)
                for counter, path in enumerate(file_paths)
            ],
            ignore_index=True,
        )
        self.save_df(df_complete)

    def save_df(self, df):
        file_extension = ".CDS"
        groups = df.groupby(df.columns[0])
        print(json.dumps({"type": "TOTAL_SAVES", "counts": groups.ngroups}))

        for group_number, (group_name, group_df) in enumerate(groups, start=1):
            print(
                json.dumps(
                    {
                        "type": "SAVE_FILE",
                        "filename": f"{group_name}{file_extension}",
                        "counter": group_number,
                    }
                )
            )

            group_df.to_csv(
                self.save_folder_path / f"{group_name}{file_extension}",
                header=False,
                index=False,
                sep="~",
            )


if __name__ == "__main__":
    try:
        grouper = ISINGrouper(sys.argv[1], sys.argv[2])
        grouper.group_by_ISIN()
    except Exception as err:
        raise err
