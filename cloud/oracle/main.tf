terraform {
  required_version = ">= 1.6.0"
  required_providers {
    oci = { source = "oracle/oci", version = "~> 7.0" }
  }
}
provider "oci" {
  region              = var.region
  config_file_profile = var.oci_profile
}
variable "region" { type = string }
variable "oci_profile" { default = "DEFAULT" }
variable "tenancy_ocid" { type = string }
variable "compartment_ocid" { type = string }
variable "ubuntu_arm64_image_ocid" { type = string }
variable "ssh_public_key" {
  type = string
  validation {
    condition     = can(regex("^(ssh-ed25519|ssh-rsa) ", var.ssh_public_key))
    error_message = "Use only your public SSH key; never paste a private key."
  }
}
variable "client_ipv4_cidr" {
  type = string
  validation {
    condition     = can(cidrhost(var.client_ipv4_cidr, 0)) && endswith(var.client_ipv4_cidr, "/32")
    error_message = "Restrict SSH to your public IPv4 /32; do not expose SSH to the world."
  }
}
variable "confirmed_account_free_capacity" {
  type    = bool
  default = false
}
data "oci_identity_availability_domains" "available" { compartment_id = var.tenancy_ocid }
resource "oci_core_vcn" "worker" {
  compartment_id = var.compartment_ocid
  cidr_blocks    = ["10.74.0.0/16"]
  display_name   = "ReviveOS worker"
  dns_label      = "reviveos"
}
resource "oci_core_internet_gateway" "worker" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.worker.id
  display_name   = "ReviveOS worker gateway"
}
resource "oci_core_route_table" "worker" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.worker.id
  route_rules {
    network_entity_id = oci_core_internet_gateway.worker.id
    destination       = "0.0.0.0/0"
  }
}
resource "oci_core_security_list" "worker" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.worker.id
  ingress_security_rules {
    protocol = "6"
    source   = var.client_ipv4_cidr
    tcp_options {
      min = 22
      max = 22
    }
  }
  egress_security_rules {
    protocol    = "all"
    destination = "0.0.0.0/0"
  }
}
resource "oci_core_subnet" "worker" {
  compartment_id    = var.compartment_ocid
  vcn_id            = oci_core_vcn.worker.id
  cidr_block        = "10.74.1.0/24"
  security_list_ids = [oci_core_security_list.worker.id]
  route_table_id    = oci_core_route_table.worker.id
  dns_label         = "worker"
}
resource "oci_core_instance" "worker" {
  availability_domain = data.oci_identity_availability_domains.available.availability_domains[0].name
  compartment_id      = var.compartment_ocid
  display_name        = "ReviveOS Linux Worker"
  shape               = "VM.Standard.A1.Flex"
  shape_config {
    ocpus         = 2
    memory_in_gbs = 12
  }
  source_details {
    source_type             = "image"
    source_id               = var.ubuntu_arm64_image_ocid
    boot_volume_size_in_gbs = 50
  }
  create_vnic_details {
    subnet_id        = oci_core_subnet.worker.id
    assign_public_ip = true
  }
  metadata = {
    ssh_authorized_keys = var.ssh_public_key
    user_data           = base64encode("#cloud-config\npackage_update: true\npackages:\n  - python3\n  - python3-venv\n  - ffmpeg\n")
  }
  lifecycle {
    precondition {
      condition     = var.confirmed_account_free_capacity
      error_message = "First check home region, Always Free eligibility, total existing CPU/RAM/storage/IP/traffic usage and all plan costs in OCI. No VM may be created without that account-specific check."
    }
  }
}
output "worker_ip" { value = oci_core_instance.worker.public_ip }
