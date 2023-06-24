import pandas as pd
from pathlib import Path
import sys
import json
import logging

logging.basicConfig(
    filename="sheet_merger.log",
    filemode="w",
    format="%(name)s - %(levelname)s - %(message)s",
    level=logging.INFO,
)


class SpreadsheetMerger:
    def __init__(self, input_source_path, save_file_path):
        self.folder_path = Path(input_source_path)
        self.columns_file_path = Path(__file__).parent.joinpath("columns.txt")
        self.save_file_path = Path(save_file_path)
        self.columns = None

    def merge_spreadsheets(self):
        column_names = self.read_column_names()
        file_paths = [
            *self.folder_path.glob("*.csv"),
            *self.folder_path.glob("*.xlsx"),
            *self.folder_path.glob("*.xls"),
        ]
        sys.out.write(
            json.dumps({"type": "TOTAL_READS", "counts": len(file_paths)})
        )
        file_paths = [str(path) for path in file_paths]

        df_complete = pd.concat(
            [
                self.get_df(path, column_names, counter)
                for counter, path in enumerate(file_paths)
            ],
            ignore_index=True,
        )
        self.save_df(df_complete)

    def read_column_names(self):
        if not self.columns:
            logging.info(
                f"Reading column names from file: {self.columns_file_path}"
            )
            with open(self.columns_file_path, "r") as f:
                json_data = json.load(f)
                # Apply strip() and uppercase conversion to keys
            json_data = {
                key.strip().upper(): value for key, value in json_data.items()
            }

            # Apply strip() and uppercase conversion to values in lists
            for key, value in json_data.items():
                if isinstance(value, list):
                    json_data[key] = [item.strip().upper() for item in value]
                else:
                    error_msg = "Expected a list in the columns."
                    logging.error(error_msg)
                    raise TypeError("Expected a list in the columns.")
            logging.info("Column names read and processed successfully")
            self.columns = json_data
        return self.columns

    def get_df(self, path, column_names, counter):
        logging.info(f"Reading file from path: {path}")
        sys.out.write(
            json.dumps(
                {"type": "READ_FILE", "filename": path, "counter": counter}
            )
        )
        if path.endswith(".csv"):
            df = pd.read_csv(path, dtype=str)
        elif path.endswith(".xlsx") or path.endswith(".xls"):
            df = pd.read_excel(path, dtype=str)
        else:
            logging.error("Error: Unsupported file type")
            raise ValueError("Error: Unsupported file type")
        df.columns = [col.strip().upper() for col in df.columns]
        logging.info(
            f"path={path}, column_names={column_names}, DataFrame columns={df.columns}"
        )
        # df = df[df.PAN.notna()]      remove rows with PAN null
        new_df = pd.DataFrame()
        for key, value in column_names.items():
            if key in df.columns:
                new_df[key] = df[key]
            elif isinstance(value, list):
                common_columns = list(set(df.columns) & set(value))
                if len(common_columns) > 0:
                    new_df[key] = df[common_columns[0]]
                else:
                    logging.error(
                        f"No common column found in DataFrame for key '{key}' and value '{value}'"
                    )
                    raise ValueError(
                        f"Error: No common column found in DataFrame for key '{key}' and value '{value}'"
                    )
            else:
                logging.info(
                    f"Error: Column '{key}' or '{value}' not found in DataFrame"
                )
                raise ValueError(
                    f"Error: Column '{key}' or '{value}' not found in DataFrame"
                )
        logging.info("Columns processed successfully")
        new_df["FILENAME"] = Path(path).name[:9]
        logging.info(
            f"Returning processed DataFrame with columns: {list(new_df.columns)}"
        )
        return new_df

    def save_df(self, df):
        file_extension = Path(self.save_file_path).suffix.lower()
        sys.out.write(json.dumps({"type": "TOTAL_SAVES", "counts": 1}))
        sys.out.write(
            json.dumps(
                {
                    "type": "SAVE_FILE",
                    "filename": self.save_file_path,
                    "counter": 1,
                }
            )
        )
        if file_extension in (".xlsx", ".xls"):
            df.to_excel(self.save_file_path, index=False)
        elif file_extension == ".csv":
            df.to_csv(self.save_file_path, index=False)
        else:
            raise ValueError(
                "Invalid file extension. Supported file extensions are .xlsx, .xls and .csv"
            )
        sys.out.write(json.dumps({"type": "EXECUTION_COMPLETED", "counts": 1}))


if __name__ == "__main__":
    try:
        merger = SpreadsheetMerger(sys.argv[1], sys.argv[2])
        merger.merge_spreadsheets()
    except Exception as err:
        raise err
